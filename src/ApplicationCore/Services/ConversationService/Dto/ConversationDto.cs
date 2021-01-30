using System;
using System.Collections.Generic;
using Microsoft.Nnn.ApplicationCore.Services.MessageService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.ConversationService.Dto
{
    public class ConversationDto
    {
        public Guid Id { get; set; }
        public bool IsUnRead { get; set; }
        public MessageUserDto Sender { get; set; }
        public MessageUserDto Receiver { get; set; }
        public List<MessageDto> Messages { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}