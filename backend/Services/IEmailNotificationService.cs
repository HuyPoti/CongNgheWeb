namespace backend.Services;

public interface IEmailNotificationService
{
    Task SendOrderConfirmedEmail(Guid orderId);
    Task SendOrderShippingEmail(Guid orderId);
    Task SendOrderDeliveredEmail(Guid orderId);
    Task SendReturnProcessedEmail(Guid returnId);
}
