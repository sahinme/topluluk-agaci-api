using System;

namespace Microsoft.Nnn.ApplicationCore.Services.ConversationService.Dto
{
    public class CreateConversationDto
    {
        public Guid SenderId { get; set; }
        
        public Guid ReceiverId { get; set; }
    }
}