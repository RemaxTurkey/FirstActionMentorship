using Application.Services.Base;
using Application.UnitOfWorks;
using RemaxSiteService.Notification.Abstractions;
using RemaxSiteService.Notification.DTOs;

namespace Application.Notifications;

public class SendTestNotification : BaseSvc<SendTestNotification.Request, SendTestNotification.Response>
{
    private readonly INotificationFactory _notificationFactory;

    public SendTestNotification(IServiceProvider serviceProvider, INotificationFactory notificationFactory) : base(
        serviceProvider)
    {
        _notificationFactory = notificationFactory;
    }

    protected override async Task<Response> _InvokeAsync(GenericUoW uow, Request req)
    {
        var request = new EmailNotificationRequest
        {
            // Gerçek veri doldur
            To = new string[] { "mehmetcem@remax.com.tr" },
            Subject = "Test Email",
            Attachments = null,
            Body = "This is a test email body.",
            IsHtml = false,
            Title = "Test Email Title"
        };
        
        var test = await _notificationFactory.SendNotification(request);
        
        var request2 = new SmsNotificationRequest
        {
            Message = "Test Sms Message",
            PhoneNumber = "5345246310"
        };
        
        var test2 = await _notificationFactory.SendNotification(request2);
        
        var request3 = new MobileNotificationRequest
        {
            Token = new List<string> { "ExponentPushToken[Me6yZPL4ui_aJoTzB6vIMc]" },
            Title = "Test Mobile Notification",
            Message = "This is a test mobile notification message.",
            Badge = 1,
            Data = null
        };

        return new();
    }

    public record Request;

    public record Response;
}