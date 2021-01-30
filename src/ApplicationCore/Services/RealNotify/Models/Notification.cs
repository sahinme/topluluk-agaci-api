using Microsoft.Nnn.ApplicationCore.Services.RealNotify.Abstraction;

namespace Microsoft.Nnn.ApplicationCore.Services.RealNotify.Models
{
    public class Notification<T> : INotification
    {
        public Notifications.NotificationType NotificationType { get; set; }
        public T Payload { get; set; }
    }
}