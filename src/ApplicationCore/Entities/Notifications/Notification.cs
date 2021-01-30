using System;
using System.ComponentModel;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.Notifications
{
    public class Notification:BaseEntity,IAggregateRoot
    {
        public string Content { get; set; }
        public NotifyContentType Type { get; set; }
        public Guid TargetId { get; set; }
        public Guid OwnerUserId { get; set; }
        public string TargetName { get; set; }
        public string ImgPath { get; set; }
        [DefaultValue(false)]
        public bool IsRead { get; set; }
    }

}