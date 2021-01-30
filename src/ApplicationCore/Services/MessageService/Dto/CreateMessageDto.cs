using System;

namespace Microsoft.Nnn.ApplicationCore.Services.MessageService.Dto
{
    public class CreateMessageDto
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid ConversationId { get; set; }
        public string Content { get; set; }
    }
}