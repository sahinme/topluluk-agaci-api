using System;
using Microsoft.Nnn.ApplicationCore.Entities.Notifications;

namespace Microsoft.Nnn.ApplicationCore.Services.NotificationService.Dto
{
    public class CreateNotificationDto
    {
        public string Content { get; set; }
        public NotifyContentType Type { get; set; }
        public Guid TargetId { get; set; }
        public Guid OwnerUserId { get; set; }
    }
}