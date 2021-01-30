using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.Comments;
using Microsoft.Nnn.ApplicationCore.Entities.CommunityUsers;
using Microsoft.Nnn.ApplicationCore.Entities.ModeratorOperations;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.CommentService;
using Microsoft.Nnn.ApplicationCore.Services.CommentService.Dto;
using Microsoft.Nnn.Web.Identity;

namespace Microsoft.Nnn.Web.Controllers.Api
{
    public class CommentController:BaseApiController
    {
        private readonly ICommentAppService _commentAppService;
        private readonly IAsyncRepository<Comment> _commentRepository;
        private readonly IAsyncRepository<CommunityUser> _communityUserRepository;
        private readonly IAsyncRepository<ModeratorOperation> _operationRepository;
        private readonly IAsyncRepository<Post> _postRepository;

        public CommentController(ICommentAppService commentAppService,IAsyncRepository<Comment> commentRepository,
            IAsyncRepository<CommunityUser> communityUserRepository,IAsyncRepository<Post> postRepository,
            IAsyncRepository<ModeratorOperation> operationRepository)
        {
            _commentAppService = commentAppService;
            _commentRepository = commentRepository;
            _communityUserRepository = communityUserRepository;
            _operationRepository = operationRepository;
            _postRepository = postRepository;
        }
        
         [Authorize]
         [HttpPost]
         public async Task<IActionResult> Create(CreateCommentDto input)
         {
             var token = GetToken();
             var userId = LoginHelper.GetClaim(token, "UserId");
             
             input.UserId = Guid.Parse(userId);
             
             var result = await _commentAppService.CreateComment(input);
             bool status = result.Id != Guid.Empty;
             return Ok(new{status});
         }

        [HttpGet]
        public async Task<IActionResult> GetPostComments(Guid postId)
        {
            var comments = await _commentAppService.GetPostComments(postId);
            return Ok(comments);
        }

//        [Authorize]
//        [HttpPut]
//        public async Task<IActionResult> UpdateComment(UpdateComment input)
//        {
//            var createdComment = await _commentAppService.UpdateComment(input);
//            return Ok(createdComment);
//        }
        
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment.UserId != Guid.Parse(userId))
            {
                return Unauthorized();
            }
            await _commentAppService.Delete(id);
            return Ok();
        }
        
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> ModeratorDelete(Guid id)
        {
            var token = GetToken();
            if (!String.IsNullOrEmpty(token))
            {
                var userId = LoginHelper.GetClaim(token, "UserId");
                var comment = await _commentRepository.GetByIdAsync(id);
                var post = await _postRepository.GetByIdAsync(comment.PostId);
                var user = await _communityUserRepository.GetAll()
                    .FirstOrDefaultAsync(x =>
                        x.IsAdmin && !x.IsDeleted && x.CommunityId == post.CommunityId &&
                        x.UserId == Guid.Parse(userId));    
                if (user == null) return Unauthorized();
                await _commentAppService.Delete(id);
                var operation = new ModeratorOperation
                {
                    CommunityId = post.CommunityId,
                    ModeratorId = Guid.Parse(userId),
                    PostId = post.Id,
                    UserId = post.UserId,
                    Operation = "COMMENT_DELETED"
                };
                await _operationRepository.AddAsync(operation); 
                return Ok(new {success=true});
            }

            return Unauthorized();
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Like(Guid commentId)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");
            
            var result = await _commentAppService.Like(Guid.Parse(userId), commentId);
            return Ok(result);
        }
        
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Unlike(Guid commentId)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");
            
            await _commentAppService.Unlike(Guid.Parse(userId), commentId);
            return Ok();
        }
    }
}