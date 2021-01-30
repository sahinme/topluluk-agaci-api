using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.Conversations;
using Microsoft.Nnn.ApplicationCore.Entities.Messages;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.ConversationService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.MessageService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.ConversationService
{
    public class ConversationAppService:IConversationAppService
    {
        private readonly IAsyncRepository<Conversation> _conversionRepository;
        private readonly IAsyncRepository<Message> _messageRepository;

        public ConversationAppService(IAsyncRepository<Conversation> conversionRepository,IAsyncRepository<Message> messageRepository)
        {
            _conversionRepository = conversionRepository;
            _messageRepository = messageRepository;
        }
        
        public async Task<Conversation> Create(CreateConversationDto input)
        {
            var model = new Conversation
            {
                SenderId = input.SenderId,
                ReceiverId = input.ReceiverId
            };
            await _conversionRepository.AddAsync(model);
            return model;
        }

        public async Task<List<ConversationDto>> GetAll(Guid userId)
        {
            var result = await _conversionRepository.GetAll().Where(x => x.IsDeleted == false &&
                                                                         (x.ReceiverId == userId || x.SenderId == userId
                                                                         ))
                .Include(x => x.Receiver).Include(x => x.Sender)
                .Select(x => new ConversationDto
                {
                    Id = x.Id,
                    IsUnRead = x.Messages.Any(i=>i.IsRead==false  && i.Receiver.Id ==userId ),
                    Receiver = new MessageUserDto
                    {
                        Id = x.Receiver.Id,
                        Username = x.Receiver.Username,
                        LogoPath = x.Receiver.ProfileImagePath == null ? null : BlobService.BlobService.GetImageUrl(x.Receiver.ProfileImagePath)
                    },
                    Sender = new MessageUserDto
                    {
                        Id = x.Sender.Id,
                        Username = x.Sender.Username,
                        LogoPath = x.Sender.ProfileImagePath==null ? null : BlobService.BlobService.GetImageUrl(x.Sender.ProfileImagePath)
                    },
                    CreatedDate = x.CreatedDate
                }).OrderByDescending(p=>p.Id).ToListAsync();
            return result;
        }

        public async Task<ConversationDto> GetById(Guid id,Guid userId)
        {
            var result = await _conversionRepository.GetAll().Where(x => x.IsDeleted == false && x.Id==id)
                .Include(x => x.Receiver).Include(x => x.Sender)
                .Select(x => new ConversationDto
                {
                    Id = x.Id,
                    Receiver = new MessageUserDto
                    {
                        Id = x.Receiver.Id,
                        Username = x.Receiver.Username,
                        LogoPath = BlobService.BlobService.GetImageUrl(x.Receiver.ProfileImagePath)
                    },
                    Sender = new MessageUserDto
                    {
                        Id = x.Sender.Id,
                        Username = x.Sender.Username,
                        LogoPath = BlobService.BlobService.GetImageUrl(x.Sender.ProfileImagePath)
                    },
                    Messages = x.Messages.Select(m=> new MessageDto
                    {
                        Id = m.Id,
                        Content = m.Content,
                        IsRead = m.IsRead,
                        CreatedDate = m.CreatedDate,
                        User = new MessageUserDto
                        {
                            Id = m.Sender.Id,
                            Username = m.Sender.Username,
                            LogoPath = BlobService.BlobService.GetImageUrl(m.Sender.ProfileImagePath)
                        }
                    }).ToList(),
                    CreatedDate = x.CreatedDate
                }).FirstOrDefaultAsync();
            var messages = await _messageRepository.GetAll()
                .Where(q => !q.IsRead && q.ConversationId == id && q.ReceiverId == userId)
                .ToListAsync();
            foreach (var message in messages)
            {
                message.IsRead = true;
                await _messageRepository.UpdateAsync(message);
            }
            return result;
        }

        public async Task Delete(Guid id)
        {
            var result = await _conversionRepository.GetAll()
                .FirstOrDefaultAsync(x => x.IsDeleted == false && x.Id == id);
            result.IsDeleted = true;
            await _conversionRepository.UpdateAsync(result);
        }
    }
}