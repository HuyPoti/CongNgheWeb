using System.Net;
using System.Security.Cryptography;
using System.Text;
using backend.DTOs;

namespace backend.Services;

public interface IVnPayService
{
    string CreatePaymentUrl(VnPayRequestDto dto, HttpContext context);
    VnPayResponseDto ProcessCallback(IQueryCollection query);
}

public class VnPayService : IVnPayService
{
    private readonly IConfiguration _config;

    public VnPayService(IConfiguration config)
    {
        _config = config;
    }

    public string CreatePaymentUrl(VnPayRequestDto dto, HttpContext context)
    {
        var tmnCode = _config["VnPay:TmnCode"];
        var hashSecret = _config["VnPay:HashSecret"];
        var baseUrl = _config["VnPay:BaseUrl"];
        var returnUrl = _config["VnPay:ReturnUrl"];

        var vnpay = new VnPayLibrary();

        vnpay.AddRequestData("vnp_Version", "2.1.0");
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", tmnCode!);
        vnpay.AddRequestData("vnp_Amount", ((long)(dto.Amount * 100)).ToString());
        vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", dto.Description);
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ReturnUrl", returnUrl!);
        vnpay.AddRequestData("vnp_TxnRef", dto.OrderId.ToString());

        return vnpay.CreateRequestUrl(baseUrl!, hashSecret!);
    }

    public VnPayResponseDto ProcessCallback(IQueryCollection query)
    {
        var hashSecret = _config["VnPay:HashSecret"];
        var vnpay = new VnPayLibrary();

        foreach (var (key, value) in query)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value!);
            }
        }

        var vnp_SecureHash = query["vnp_SecureHash"];
        bool isValidSignature = vnpay.ValidateSignature(vnp_SecureHash!, hashSecret!);

        if (!isValidSignature)
        {
            return new VnPayResponseDto { Success = false, Message = "Invalid signature" };
        }

        var responseCode = query["vnp_ResponseCode"];
        bool isSuccess = responseCode == "00";

        return new VnPayResponseDto
        {
            Success = isSuccess,
            Message = isSuccess ? "Success" : "Payment failed with code " + responseCode,
            OrderId = query["vnp_TxnRef"],
            TransactionNo = query["vnp_TransactionNo"]
        };
    }
}

public class VnPayLibrary
{
    private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

    public void AddRequestData(string key, string value) => _requestData.Add(key, value);
    public void AddResponseData(string key, string value) => _responseData.Add(key, value);

    public string CreateRequestUrl(string baseUrl, string hashSecret)
    {
        var data = new StringBuilder();
        foreach (var kv in _requestData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }

        string queryString = data.ToString();
        baseUrl += "?" + queryString;
        string signData = queryString.Remove(data.Length - 1);
        string vnp_SecureHash = HmacSHA512(hashSecret, signData);
        baseUrl += "vnp_SecureHash=" + vnp_SecureHash;

        return baseUrl;
    }

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        string rspRaw = GetResponseRaw();
        string myChecksum = HmacSHA512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetResponseRaw()
    {
        var data = new StringBuilder();
        if (_responseData.ContainsKey("vnp_SecureHashType")) _responseData.Remove("vnp_SecureHashType");
        if (_responseData.ContainsKey("vnp_SecureHash")) _responseData.Remove("vnp_SecureHash");

        foreach (var kv in _responseData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }

        if (data.Length > 0) data.Remove(data.Length - 1, 1);
        return data.ToString();
    }

    private string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            byte[] hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }
        return hash.ToString();
    }
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        return string.CompareOrdinal(x, y);
    }
}
