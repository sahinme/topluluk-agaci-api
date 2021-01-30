using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Nnn.ApplicationCore.Entities.Conversations;
using Microsoft.Nnn.ApplicationCore.Services.ConversationService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.ConversationService
{
    public interface IConversationAppService
    {
        Task<Conversation> Create(CreateConversationDto input);
        Task<List<ConversationDto>> GetAll(Guid userId);
        Task<ConversationDto> GetById(Guid id,Guid userId);
        Task Delete(Guid id);
    }
}