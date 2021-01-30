using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.Notifications;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.NotificationService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.NotificationService
{
    public class NotificationAppService:INotificationAppService
    {
        private readonly IAsyncRepository<Notification> _notificationRepository;

        public NotificationAppService(IAsyncRepository<Notification> notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<List<NotificationDto>> GetUserNotifications(Guid userId)
        {
            var result = await _notificationRepository.GetAll()
                .Where(x => x.IsDeleted == false && x.OwnerUserId == userId)
                .Select(x => new NotificationDto
                {
                    Id = x.Id,
                    Content = x.Content,
                    TargetId = x.TargetId,
                    TargetName = x.TargetName,
                    CreatedDate = x.CreatedDate,
                    IsRead = x.IsRead,
                    ImgPath = BlobService.BlobService.GetImageUrl(x.ImgPath),
                    Type = x.Type,
                }).OrderByDescending(x=>x.CreatedDate).ToListAsync();
            return result;
        }

        public async Task<long> GetUnReads(Guid userId)
        {
            var result = await _notificationRepository.GetAll()
                .Where(x => x.IsDeleted == false && x.OwnerUserId == userId && x.IsRead == false).CountAsync();
            return result;
        }

        public async Task MarkAsRead(Guid[] ids)
        {
            foreach (var id in ids)
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }
    }
}