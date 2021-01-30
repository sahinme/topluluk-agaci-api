using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.CommunityUsers;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.CommunityService;
using Microsoft.Nnn.ApplicationCore.Services.CommunityService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.Dto;
using Microsoft.Nnn.Web.Identity;

namespace Microsoft.Nnn.Web.Controllers.Api
{
    public class CommunityController:BaseApiController
    {
        private readonly ICommunityAppService _communityAppService;
        private readonly IAsyncRepository<CommunityUser> _communityUserRepository;

        public CommunityController(ICommunityAppService communityAppService,IAsyncRepository<CommunityUser> communityUserRepository)
        {
            _communityAppService = communityAppService;
            _communityUserRepository = communityUserRepository;
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateCommunity(CreateCommunity input)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");
            input.UserId = Guid.Parse(userId);
            var result = await _communityAppService.CreateCommunity(input);
            return Ok(result);
        }
        
        //[Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _communityAppService.GetAll();
            return Ok(result);
        }
        
        [HttpGet]
        public async Task<IActionResult> OfModerators()
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");
            
            var result = await _communityAppService.OfModerators(Guid.Parse(userId));
            return Ok(result);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddModerator(AddModeratorDto input)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            if (userId == null || token==null || String.IsNullOrEmpty(token) ) return Unauthorized();

            var isModerator = await _communityUserRepository.GetAll()
                .Where(x => x.UserId == Guid.Parse(userId) && x.IsAdmin && x.Community.Slug ==input.CommunitySlug &&  !x.IsDeleted)
                .FirstOrDefaultAsync();
            if (isModerator == null) return Unauthorized();

            input.RequesterModeratorId = Guid.Parse(userId);
            var result = await _communityAppService.AddModerator(input);
            return Ok(result);
        }
        
        [HttpGet]
        public async Task<IActionResult> Get(string slug)
        {
            var token = GetToken();
            string userId = null;
            bool isTokenEmpty = String.IsNullOrEmpty(token);
            if ( !isTokenEmpty )
            {
                 userId = LoginHelper.GetClaim(token, "UserId");

            }
            
            var result = await _communityAppService.GetById(slug,userId == null ? Guid.Empty : Guid.Parse(userId));
            return Ok(result);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] PageDtoCommunity input)
        {
            var token = GetToken();
            string userId = null;
            bool isTokenEmpty = String.IsNullOrEmpty(token);
            if ( !isTokenEmpty )
            {
                userId = LoginHelper.GetClaim(token, "UserId");

            }

            if (userId != null) input.UserId = Guid.Parse(userId);
            
            var result = await _communityAppService.GetPosts(input);
            return Ok(result);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetPopulars(Guid userId)
        {
            var result = await _communityAppService.GetPopulars(userId);
            return Ok(result);
        }
        
        [HttpGet]
        public async Task<IActionResult> Search(string text)
        {
            var result = await _communityAppService.Search(text);
            return Ok(result);
        }
        
        [HttpGet]
        public async Task<IActionResult> Users(string slug)
        {
            var result = await _communityAppService.Users(slug);
            return Ok(result);
        }
        
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update([FromForm] UpdateCommunity input)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId)) return Unauthorized();
            
            input.ModeratorId = Guid.Parse(userId);
            
            var result = await _communityAppService.Update(input);
            return Ok(result);
        }

        
    }
}