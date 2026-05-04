using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IEmailService _emailService;
    private readonly AppDbContext _context;

    public EmailNotificationService(IEmailService emailService, AppDbContext context)
    {
        _emailService = emailService;
        _context = context;
    }

    public async Task SendOrderConfirmedEmail(Guid orderId)
    {
        var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order?.User != null)
        {
            string subject = $"Xác nhận đơn hàng #{order.OrderCode}";
            string body = $"Chào {order.User.FullName}, đơn hàng của bạn đã được xác nhận thành công.";
            await _emailService.SendEmailAsync(order.User.Email, subject, body);
        }
    }

    public async Task SendOrderShippingEmail(Guid orderId)
    {
        var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order?.User != null)
        {
            string subject = $"Đơn hàng #{order.OrderCode} đang được vận chuyển";
            string body = $"Đơn hàng của bạn đã được bàn giao cho đơn vị vận chuyển.";
            await _emailService.SendEmailAsync(order.User.Email, subject, body);
        }
    }

    public async Task SendOrderDeliveredEmail(Guid orderId)
    {
        var order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order?.User != null)
        {
            string subject = $"Đơn hàng #{order.OrderCode} đã giao thành công";
            string body = $"Cảm ơn bạn đã mua hàng. Bạn có thể để lại đánh giá cho sản phẩm ngay bây giờ.";
            await _emailService.SendEmailAsync(order.User.Email, subject, body);
        }
    }

    public async Task SendReturnProcessedEmail(Guid returnId)
    {
        var request = await _context.ReturnRequests
            .Include(r => r.User)
            .Include(r => r.Order)
            .FirstOrDefaultAsync(r => r.ReturnId == returnId);

        if (request?.User != null)
        {
            string subject = $"Yêu cầu đổi trả đơn hàng #{request.Order?.OrderCode} đã được xử lý";
            string statusText = request.Status == "approved" ? "được CHẤP NHẬN" : "bị TỪ CHỐI";
            string body = $"Yêu cầu đổi trả của bạn đã {statusText}. Ghi chú từ cửa hàng: {request.AdminNote}";
            await _emailService.SendEmailAsync(request.User.Email, subject, body);
        }
    }
}
