using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Nnn.ApplicationCore.Services.NotificationService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.NotificationService
{
    public interface INotificationAppService
    {
        Task<List<NotificationDto>> GetUserNotifications(Guid userId);
        Task<long> GetUnReads(Guid userId);
        Task MarkAsRead(Guid[] ids);
    }
}