using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.PostService;
using Microsoft.Nnn.ApplicationCore.Services.RealNotify;
using Microsoft.Nnn.ApplicationCore.Services.RealNotify.Models;

namespace Microsoft.Nnn.Web.Controllers.Api
{
    public class RealNotifyController:BaseApiController
    {
        private readonly IPostAppService _postAppService;
        private readonly IUserService _userAppService;
        IHubContext<NotificationsHub> _hubContext;
        
        public  RealNotifyController(IPostAppService postAppService,IUserService userAppService,IHubContext<NotificationsHub> hubContext)
        {
            _postAppService = postAppService;
            _userAppService = userAppService;
            _hubContext = hubContext;
        }
        
//        [Authorize]
//        [HttpPost("{id}/toggleLike")]
//        public async Task<ActionResult> ToggleLike(Guid id)
//        {
//            var userId = HttpContext.User.Identity.Name;
//
//            var story = await _postAppService.GetPostById(id,null);
//            //if (userId == story.OwnerId) return BadRequest("You can't like your own story");
//
//            var user = await _userAppService.GetByUsername(userId);
//            //var existingLike = story.Likes.Find(l => l.UserId == userId);
//            var payload = new LikeRelatedPayload
//            {
//                Username = user.Username,
//                StoryTitle = user.Username+ " "+ story.Id + " id li postu begendi"
//            };
//            
//           await _hubContext.Clients.User(story.UserInfo.UserName).SendAsync(
//                "notification",
//                new Notification<LikeRelatedPayload>
//                {
//                    NotificationType = Notifications.NotificationType.LIKE,
//                    Payload = payload
//                }
//            );
//            
//            if (story != null)
//            {
//                hubContext.Clients.User(story.OwnerId).SendAsync(
//                    "notification",
//                    new Notification<LikeRelatedPayload>
//                    {
//                        NotificationType = Notifications.NotificationType.LIKE,
//                        Payload = payload
//                    }
//                );
//                likeRepository.Add(new Like
//                {
//                    UserId = userId,
//                    StoryId = id
//                });
//            }
//            else 
//            {
//                hubContext.Clients.User(story.OwnerId).SendAsync(
//                    "notification",
//                    new Notification<LikeRelatedPayload>
//                    {
//                        NotificationType = Notifications.NotificationType.UNLIKE,
//                        Payload = payload
//                    }
//                );
//                likeRepository.Delete(existingLike);
//            }
//            likeRepository.Commit();
            //return NoContent();
        //}
    }
}