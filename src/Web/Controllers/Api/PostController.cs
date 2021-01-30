using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.CommunityUsers;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.PostVotes;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.Dto;
using Microsoft.Nnn.ApplicationCore.Services.PostAppService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.PostService;
using Microsoft.Nnn.ApplicationCore.Services.PostService.Dto;
using Microsoft.Nnn.Web.Identity;

namespace Microsoft.Nnn.Web.Controllers.Api
{
    public class PostController:BaseApiController
    {
        private readonly IPostAppService _postAppService;
        private readonly IAsyncRepository<Post> _postRepository;
        private readonly IAsyncRepository<CommunityUser> _communityUserRepository;

        public PostController(IPostAppService postAppService, IAsyncRepository<Post> postRepository,
            IAsyncRepository<CommunityUser> communityUserRepository)
        {
            _postAppService = postAppService;
            _postRepository = postRepository;
            _communityUserRepository = communityUserRepository;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto input)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            if (userId==null) return Unauthorized();

            input.UserId = Guid.Parse(userId);
            
            var createdPost = await _postAppService.CreatePost(input);
            return Ok(createdPost);
        }

        [HttpGet]
        public async Task<IActionResult> Get(string slug)
        {
            var token = GetToken();
            string userId = null;
            if (!string.IsNullOrEmpty(token))
            {  userId = LoginHelper.GetClaim(token, "UserId");
            }
            
            var post = await _postAppService.GetPostById(slug, userId == null ? Guid.Empty : Guid.Parse(userId));
            return Ok(post);
        }
        
        
        [HttpGet]
        public async Task<IActionResult> GetUserPosts([FromQuery] IdOrUsernameDto input)
        {
            var token = GetToken();
            if (!String.IsNullOrEmpty(token))
            {
                var userId = LoginHelper.GetClaim(token, "UserId");
                input.Id = Guid.Parse(userId);
            }

            if (input.Id == null) input.Id = Guid.Empty;
           
            var post = await _postAppService.GetUserPosts(input);
            return Ok(post);
        }
        
//        [HttpGet]
//        public async Task<IActionResult> HomePosts(Guid userId)
//        {
//            var post = await _postAppService.HomePosts(userId);
//            return Ok(post);
//        }
        
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PagedHomePosts([FromQuery] PaginationParams input)
        {
            var token = GetToken();
            if (!String.IsNullOrEmpty(token))
            {
                var userId = LoginHelper.GetClaim(token, "UserId");
                input.EntityId = Guid.Parse(userId);
            }

            if (input.EntityId == null) return Unauthorized();
            
            var post = await _postAppService.PagedHomePosts(input);
            return Ok(post);
        }
        
        [HttpGet]
        public async Task<IActionResult> PagedUnauthorizedHomePost([FromQuery] PaginationParams input)
        {
            var post = await _postAppService.PagedUnauthorizedHomePosts(input);
            return Ok(post);
        }
        
        [HttpGet]
        public async Task<IActionResult> DailyTrends([FromQuery] PaginationParams input)
        {
            var token = GetToken();
            var userId = Guid.Empty;
            
            if (!String.IsNullOrEmpty(token))
            {
                userId = Guid.Parse(LoginHelper.GetClaim(token, "UserId"));
                input.EntityId = userId;
            }
            var post = await _postAppService.DailyTrends(input);
            return Ok(post);
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> HandlePin(string slug,bool value)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");
            var post = await _postRepository.GetAll().Where(x => !x.IsDeleted && x.Slug == slug)
                .Include(x=>x.Community).FirstOrDefaultAsync();
            var isMod = await _communityUserRepository.GetAll().Where(x =>
                    x.UserId == Guid.Parse(userId) && x.Community.Slug == post.Community.Slug && x.IsAdmin)
                .FirstOrDefaultAsync();
            if (isMod == null) return Unauthorized();

            var result = await _postAppService.HandlePin(slug, value);
            return Ok(result);
        }
        
//        [HttpGet]
//        public async Task<IActionResult>  UnauthorizedHomePost()
//        {
//            var result = await _postAppService.UnauthorizedHomePosts();
//            return Ok(result);
//        }
        
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            var post = await _postRepository.GetByIdAsync(id);
            
            if(post.UserId != Guid.Parse(userId)) return Unauthorized();
            
            await _postAppService.Delete(id);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetSlugs()
        {
            var result = await _postAppService.GetAllPostsSlug();
            return Ok(result);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> ModeratorDelete(ModeratorDeleteDto input)
        {
            var token = GetToken();
            if (!String.IsNullOrEmpty(token))
            {
                var loggedUserId = LoginHelper.GetClaim(token, "UserId");
                input.ModeratorId = Guid.Parse(loggedUserId);
            }

            var isAdmin = await _communityUserRepository.GetAll()
                .FirstOrDefaultAsync(x =>
                    x.IsDeleted == false && x.IsAdmin && x.UserId == input.ModeratorId && x.Community.Slug == input.Slug);
            if (isAdmin == null) return Unauthorized();
            
            await _postAppService.DeleteModerator(input);
            return Ok();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Vote(CreateVoteDto input)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            if (input.UserId != Guid.Parse(userId)) return Unauthorized();
            
            var result = await _postAppService.Vote(input);
            var success = result.Id != Guid.Empty;
            return Ok(new {success});
        }
    }
}