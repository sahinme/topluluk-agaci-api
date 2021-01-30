using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Nnn.ApplicationCore.Services.MessageService;
using Microsoft.Nnn.ApplicationCore.Services.MessageService.Dto;
using Microsoft.Nnn.Web.Identity;

namespace Microsoft.Nnn.Web.Controllers.Api
{
    public class MessageController:BaseApiController
    {
        private readonly IMessageAppService _messageAppService;
        
        public  MessageController(IMessageAppService messageAppService)
        {
            _messageAppService = messageAppService;
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateMessageDto input)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            if (input.SenderId != Guid.Parse(userId)) return Unauthorized();
            
            var result = await _messageAppService.Create(input);
            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUnReads()
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            var count = await _messageAppService.GetUnreadCount(Guid.Parse(userId));
            return Ok( new {error=false,count});
        }
        
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> MarkAsRead(Guid[] ids)
        {
            await _messageAppService.MarkAsRead(ids);
            return Ok();
        }
    }
}