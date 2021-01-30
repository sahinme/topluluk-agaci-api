using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Nnn.ApplicationCore.Services.ConversationService;
using Microsoft.Nnn.ApplicationCore.Services.ConversationService.Dto;
using Microsoft.Nnn.Web.Identity;

namespace Microsoft.Nnn.Web.Controllers.Api
{
    public class ConversationController:BaseApiController
    {
        private readonly IConversationAppService _conversationAppService;
        
        public  ConversationController(IConversationAppService conversationAppService)
        {
            _conversationAppService = conversationAppService;
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateConversationDto input)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");
            if (input.SenderId != Guid.Parse(userId)) return Unauthorized();
            
            var result = await _conversationAppService.Create(input);
            return Ok(result);
        }
        
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");
            
            var result = await _conversationAppService.GetAll(Guid.Parse(userId));
            return Ok(result);
        }
        
        [Authorize]
        [HttpGet("by-id")]
        public async Task<IActionResult> Get(Guid id)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");
            
            var result = await _conversationAppService.GetById(id,Guid.Parse(userId));
            return Ok(result);
        }
    }
}