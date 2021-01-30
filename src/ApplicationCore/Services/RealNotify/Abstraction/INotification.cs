namespace Microsoft.Nnn.ApplicationCore.Services.RealNotify.Abstraction
{
    public interface INotification
    {
        Notifications.NotificationType NotificationType { get; set; }
    }
}