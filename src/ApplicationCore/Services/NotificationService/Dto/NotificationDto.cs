using System;
using Microsoft.Nnn.ApplicationCore.Entities.Notifications;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;

namespace Microsoft.Nnn.ApplicationCore.Services.NotificationService.Dto
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public NotifyContentType Type { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid TargetId { get; set; }
        public string TargetName { get; set; }
        public string ImgPath { get; set; }
        public bool IsRead { get; set; }
    }
}