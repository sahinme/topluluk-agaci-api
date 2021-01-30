using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.Conversations;
using Microsoft.Nnn.ApplicationCore.Entities.Messages;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.ConversationService;
using Microsoft.Nnn.ApplicationCore.Services.ConversationService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.MessageService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.MessageService
{
    public class MessageAppService:IMessageAppService
    {
        private readonly IAsyncRepository<Message> _messageRepository;
        private readonly IAsyncRepository<User> _userRepository;
        private readonly IConversationAppService _conversationAppService;
        private readonly IEmailSender _emailSender;

        public MessageAppService(IAsyncRepository<Message> messageRepository,IConversationAppService conversationAppService,
            IAsyncRepository<User> userRepository,IEmailSender emailSender)
        {
            _messageRepository = messageRepository;
            _conversationAppService = conversationAppService;
            _userRepository = userRepository;
            _emailSender = emailSender;
        }
        
        public async Task<Message> Create(CreateMessageDto input)
        {
          
            if (input.ConversationId == Guid.Empty)
            {
               var conversation = await _conversationAppService.Create(new CreateConversationDto
                    {ReceiverId = input.ReceiverId, SenderId = input.SenderId});
               
               var message = new Message
               {
                   SenderId = input.SenderId,
                   ReceiverId = input.ReceiverId,
                   Content = input.Content,
                   ConversationId = conversation.Id
               };
               await _messageRepository.AddAsync(message);
               return message;
            }
            var model = new Message
            {
                SenderId = input.SenderId,
                ReceiverId = input.ReceiverId,
                Content = input.Content,
                ConversationId = input.ConversationId
            };
            await _messageRepository.AddAsync(model);
            
            var receiver = await _userRepository.GetByIdAsync(input.ReceiverId);
            var sender = await _userRepository.GetByIdAsync(input.SenderId);
            var emailBody = sender.Username + " isimli kullanıcı size mesaj gönderdi";
            await _emailSender.SendEmail(receiver.EmailAddress, "Yeni mesajınız var", emailBody);
            return model;
        }

        public async Task<long> GetUnreadCount(Guid userId)
        {
            var count = await _messageRepository.GetAll()
                .CountAsync(x => x.ReceiverId == userId && !x.IsRead && !x.IsDeleted);
            return count;
        }
        
        public async Task MarkAsRead(Guid[] ids)
        {
            foreach (var id in ids)
            {
                var notification = await _messageRepository.GetByIdAsync(id);
                notification.IsRead = true;
                await _messageRepository.UpdateAsync(notification);
            }
        }
    }
}