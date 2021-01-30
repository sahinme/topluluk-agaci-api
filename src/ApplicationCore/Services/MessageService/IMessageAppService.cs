using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Nnn.ApplicationCore.Entities.Messages;
using Microsoft.Nnn.ApplicationCore.Services.MessageService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.MessageService
{
    public interface IMessageAppService
    {
        Task<Message> Create(CreateMessageDto input);
        Task<long> GetUnreadCount(Guid userId);
        Task MarkAsRead(Guid[] ids);
    }
}