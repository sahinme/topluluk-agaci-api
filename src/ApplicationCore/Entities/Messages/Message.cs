using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Nnn.ApplicationCore.Entities.Conversations;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.Messages
{
    public class Message:BaseEntity,IAggregateRoot
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid ConversationId { get; set; }
        public string Content { get; set; }
        
        [DefaultValue(false)]
        public bool IsRead { get; set; }
        
        [ForeignKey(nameof(SenderId))]
        public virtual User Sender { get; set; }
        
        [ForeignKey(nameof(ReceiverId))]
        public virtual User Receiver { get; set; }

        public Conversation Conversation { get; set; }
    }
}