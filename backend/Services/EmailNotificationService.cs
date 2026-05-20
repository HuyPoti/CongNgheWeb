using backend.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _uow;

    public EmailNotificationService(IEmailService emailService, IUnitOfWork uow)
    {
        _emailService = emailService;
        _uow = uow;
    }

    public async Task SendOrderConfirmedEmail(Guid orderId)
    {
        var order = await _uow.Orders.Query().Include(o => o.User).FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order?.User != null)
        {
            string subject = $"Xác nhận đơn hàng #{order.OrderCode}";
            string body = $"Chào {order.User.FullName}, đơn hàng của bạn đã được xác nhận thành công.";
            await _emailService.SendEmailAsync(order.User.Email, subject, body);
        }
    }

    public async Task SendOrderShippingEmail(Guid orderId)
    {
        var order = await _uow.Orders.Query().Include(o => o.User).FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order?.User != null)
        {
            string subject = $"Đơn hàng #{order.OrderCode} đang được vận chuyển";
            string body = $"Đơn hàng của bạn đã được bàn giao cho đơn vị vận chuyển.";
            await _emailService.SendEmailAsync(order.User.Email, subject, body);
        }
    }

    public async Task SendOrderDeliveredEmail(Guid orderId)
    {
        var order = await _uow.Orders.Query().Include(o => o.User).FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order?.User != null)
        {
            string subject = $"Đơn hàng #{order.OrderCode} đã giao thành công";
            string body = $"Cảm ơn bạn đã mua hàng. Bạn có thể để lại đánh giá cho sản phẩm ngay bây giờ.";
            await _emailService.SendEmailAsync(order.User.Email, subject, body);
        }
    }

    public async Task SendReturnProcessedEmail(Guid returnId)
    {
        var request = await _uow.ReturnRequests.Query()
            .Include(r => r.User)
            .Include(r => r.Order)
            .Include(r => r.Items)
                .ThenInclude(i => i.OrderItem)
                    .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(r => r.ReturnId == returnId);

        if (request?.User == null) return;

        var (statusLabel, statusColor, statusIcon) = request.Status.ToLower() switch
        {
            "approved" => ("CHẤP NHẬN", "#16a34a", "✅"),
            "rejected" => ("TỪ CHỐI", "#dc2626", "❌"),
            "completed" => ("HOÀN TẤT", "#0284c7", "🎉"),
            _ => (request.Status.ToUpper(), "#6b7280", "ℹ️")
        };

        var productRows = request.Items
            .Where(i => i.OrderItem?.Product != null)
            .Select(i => $@"
                <tr>
                    <td style=""padding:10px 16px;border-bottom:1px solid #f1f5f9;font-size:14px;color:#1e293b"">{i.OrderItem!.Product!.Name}</td>
                    <td style=""padding:10px 16px;border-bottom:1px solid #f1f5f9;font-size:14px;color:#64748b;text-align:center"">{i.Quantity}</td>
                </tr>")
            .DefaultIfEmpty(@"<tr><td colspan=""2"" style=""padding:10px 16px;color:#94a3b8;font-style:italic"">Không có thông tin sản phẩm</td></tr>");

        var orderCode = request.Order?.OrderCode ?? "N/A";
        var receivedDate = request.CreatedAt.AddHours(7).ToString("HH:mm, dd/MM/yyyy");
        var sentDate = DateTime.UtcNow.AddHours(7).ToString("HH:mm, dd/MM/yyyy");

        var adminNoteBlock = !string.IsNullOrWhiteSpace(request.AdminNote)
            ? $@"<div style=""margin:20px 0;padding:16px;background:#f8fafc;border-left:4px solid {statusColor};border-radius:0 8px 8px 0"">
                    <p style=""margin:0 0 4px;font-size:12px;font-weight:700;color:#64748b;text-transform:uppercase;letter-spacing:0.05em"">Phản hồi từ nhân viên</p>
                    <p style=""margin:0;font-size:14px;color:#374151;font-style:italic"">""{request.AdminNote}""</p>
                 </div>"
            : "";

        var actionBlock = request.Status.ToLower() == "approved"
            ? @"<div style=""margin:20px 0;padding:16px;background:#ecfdf5;border:1px solid #bbf7d0;border-radius:8px"">
                    <p style=""margin:0;font-size:14px;color:#166534"">👉 <strong>Bước tiếp theo:</strong> Vui lòng đóng gói và gửi trả hàng về địa chỉ cửa hàng. Chúng tôi sẽ hoàn tiền trong vòng 3-5 ngày làm việc sau khi nhận được hàng.</p>
               </div>"
            : "";

        string subject = $"[{orderCode}] Yêu cầu đổi trả {statusIcon} {statusLabel}";

        string body = $@"<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1""></head>
<body style=""margin:0;padding:0;background:#f1f5f9;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f1f5f9;padding:32px 16px"">
    <tr><td align=""center"">
      <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;width:100%"">

        <!-- Header -->
        <tr><td style=""background:{statusColor};border-radius:12px 12px 0 0;padding:32px;text-align:center"">
          <p style=""margin:0 0 8px;font-size:32px"">{statusIcon}</p>
          <h1 style=""margin:0;font-size:22px;font-weight:800;color:#ffffff;letter-spacing:-0.5px"">Yêu cầu đổi trả {statusLabel}</h1>
          <p style=""margin:8px 0 0;font-size:14px;color:rgba(255,255,255,0.85)"">Đơn hàng #{orderCode}</p>
        </td></tr>

        <!-- Body -->
        <tr><td style=""background:#ffffff;padding:32px"">
          <p style=""margin:0 0 24px;font-size:15px;color:#374151"">Xin chào <strong>{request.User.FullName}</strong>,</p>
          <p style=""margin:0 0 24px;font-size:15px;color:#374151"">
            Yêu cầu đổi trả của bạn cho đơn hàng <strong>#{orderCode}</strong> đã được xử lý với trạng thái: 
            <span style=""display:inline-block;padding:2px 10px;background:{statusColor};color:#fff;border-radius:999px;font-size:13px;font-weight:700"">{statusLabel}</span>
          </p>

          <!-- Products Table -->
          <p style=""margin:0 0 8px;font-size:12px;font-weight:700;color:#64748b;text-transform:uppercase;letter-spacing:0.05em"">📦 Sản phẩm yêu cầu đổi trả</p>
          <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border:1px solid #e2e8f0;border-radius:8px;overflow:hidden;margin-bottom:24px"">
            <thead>
              <tr style=""background:#f8fafc"">
                <th style=""padding:10px 16px;text-align:left;font-size:12px;color:#64748b;font-weight:700"">TÊN SẢN PHẨM</th>
                <th style=""padding:10px 16px;text-align:center;font-size:12px;color:#64748b;font-weight:700"">SỐ LƯỢNG</th>
              </tr>
            </thead>
            <tbody>
              {string.Join("", productRows)}
            </tbody>
          </table>

          <!-- Dates -->
          <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f8fafc;border-radius:8px;padding:16px;margin-bottom:24px"">
            <tr>
              <td style=""padding:6px 16px;font-size:13px;color:#64748b"">📅 Ngày gửi yêu cầu</td>
              <td style=""padding:6px 16px;font-size:13px;color:#1e293b;font-weight:600;text-align:right"">{receivedDate}</td>
            </tr>
            <tr>
              <td style=""padding:6px 16px;font-size:13px;color:#64748b"">⏱️ Thời gian xử lý</td>
              <td style=""padding:6px 16px;font-size:13px;color:#1e293b;font-weight:600;text-align:right"">{sentDate}</td>
            </tr>
          </table>

          {adminNoteBlock}
          {actionBlock}

          <p style=""margin:24px 0 0;font-size:13px;color:#94a3b8"">Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ bộ phận hỗ trợ của chúng tôi.</p>
        </td></tr>

        <!-- Footer -->
        <tr><td style=""background:#f8fafc;border-radius:0 0 12px 12px;padding:20px;text-align:center;border-top:1px solid #e2e8f0"">
          <p style=""margin:0;font-size:13px;color:#94a3b8"">© 2024 Cửa hàng của chúng tôi · Đội ngũ Chăm sóc Khách hàng</p>
        </td></tr>

      </table>
    </td></tr>
  </table>
</body>
</html>";

        await _emailService.SendEmailAsync(request.User.Email, subject, body);
    }
}
