using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.Comments;
using Microsoft.Nnn.ApplicationCore.Entities.CommunityUsers;
using Microsoft.Nnn.ApplicationCore.Entities.ModeratorOperations;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.Replies;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.ReplyService;
using Microsoft.Nnn.ApplicationCore.Services.ReplyService.Dto;
using Microsoft.Nnn.Web.Identity;

namespace Microsoft.Nnn.Web.Controllers.Api
{
    public class ReplyController:BaseApiController
    {
        private readonly IReplyAppService _replyAppService;
        private readonly IAsyncRepository<Reply> _replyRepository;
        private readonly IAsyncRepository<ModeratorOperation> _operationRepository;
        private readonly IAsyncRepository<CommunityUser> _communityUserRepository;
        private readonly IAsyncRepository<Comment> _commentRepository;
        private readonly IAsyncRepository<Post> _postRepository;

        public ReplyController(IReplyAppService replyAppService,IAsyncRepository<Reply> replyRepository,
            IAsyncRepository<Post> postRepository,
            IAsyncRepository<ModeratorOperation> operationRepository,
            IAsyncRepository<CommunityUser> communityUserRepository,
            IAsyncRepository<Comment> commentRepository)
        {
            _replyAppService = replyAppService;
            _replyRepository = replyRepository;
            _postRepository = postRepository;
            _operationRepository = operationRepository;
            _commentRepository = commentRepository;
            _communityUserRepository = communityUserRepository;
        }

        [Authorize]
        [HttpPost]
        public async  Task<IActionResult> CreateReply(CreateReplyDto input)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            if (input.UserId != Guid.Parse(userId)) return Unauthorized();
            
            var reply = await _replyAppService.CreateReply(input);
            bool status = reply.Id != Guid.Empty;
            return Ok(new{status});
        }
        
        [Authorize]
        [HttpPost]
        public async  Task<IActionResult> Like(Guid replyId)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            var reply = await _replyAppService.Like(Guid.Parse(userId), replyId);
            bool status = reply.Id != Guid.Empty;
            return Ok(new{status});
        }
        
        [Authorize]
        [HttpDelete]
        public async  Task<IActionResult> Unlike(Guid replyId)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            await _replyAppService.Unlike(Guid.Parse(userId), replyId);
            return Ok();
        }
        
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            var reply = await _replyRepository.GetByIdAsync(id);
            if (reply.UserId != Guid.Parse(userId)) return Unauthorized();
            
            await _replyAppService.Delete(id);
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
                var reply = await _replyRepository.GetByIdAsync(id);
                var comment = await _commentRepository.GetByIdAsync(reply.CommentId);
                var post = await _postRepository.GetByIdAsync(comment.PostId);
                
                var user = await _communityUserRepository.GetAll()
                    .FirstOrDefaultAsync(x =>
                        x.IsAdmin && !x.IsDeleted && x.CommunityId == post.CommunityId &&
                        x.UserId == Guid.Parse(userId));    
                if (user == null) return Unauthorized();
                
                await _replyAppService.Delete(id);
                
                var operation = new ModeratorOperation
                {
                    CommunityId = post.CommunityId,
                    ModeratorId = Guid.Parse(userId),
                    PostId = post.Id,
                    UserId = post.UserId,
                    Operation = "REPLY_DELETED"
                };
                await _operationRepository.AddAsync(operation); 
                return Ok(new {success=true});
            }

            return Unauthorized();
        }
    }
}