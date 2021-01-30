using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Nnn.ApplicationCore.Services.NotificationService;
using Microsoft.Nnn.ApplicationCore.Services.NotificationService.Dto;
using Microsoft.Nnn.Web.Identity;

namespace Microsoft.Nnn.Web.Controllers.Api
{
    public class NotificationController:BaseApiController
    {
        private readonly INotificationAppService _notificationAppService;

        public NotificationController(INotificationAppService notificationAppService)
        {
            _notificationAppService = notificationAppService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            var result = await _notificationAppService.GetUserNotifications(Guid.Parse(userId));
            return Ok(result);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            var result = await _notificationAppService.GetUnReads(Guid.Parse(userId));
            return Ok(new { count=result });
        }

        [HttpPut]
        public async Task<IActionResult> MarkAsRead(Guid[] ids)
        {
            await _notificationAppService.MarkAsRead(ids);
            return Ok();
        }
    }
}