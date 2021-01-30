using System;
using System.Collections.Generic;
using Microsoft.Nnn.ApplicationCore.Entities.Messages;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.Conversations
{
    public class Conversation:BaseEntity,IAggregateRoot
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }

        public User Sender { get; set; }
        public User Receiver { get; set; }
        
        public virtual ICollection<Message> Messages { get; set; }
    }
}