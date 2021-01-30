using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.Comments;
using Microsoft.Nnn.ApplicationCore.Entities.Communities;
using Microsoft.Nnn.ApplicationCore.Entities.Notifications;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.Replies;
using Microsoft.Nnn.ApplicationCore.Entities.ReplyLikes;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.ReplyService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.ReplyService
{
    public class ReplyAppService:IReplyAppService
    {
        private readonly IAsyncRepository<Reply> _replyRepository;
        private readonly IAsyncRepository<ReplyLike> _likeRepository;
        private readonly IAsyncRepository<Notification> _notificationRepository;
        private readonly IAsyncRepository<User> _userRepository;
        private readonly IAsyncRepository<Post> _postRepository;
        private readonly IAsyncRepository<Comment> _commentRepository;
        private readonly IAsyncRepository<Community> _communityRepository;

        public ReplyAppService(IAsyncRepository<Reply> replyRepository, IAsyncRepository<ReplyLike> likeRepository,
            IAsyncRepository<Notification> notificationRepository,IAsyncRepository<User> userRepository,
            IAsyncRepository<Post> postRepository,IAsyncRepository<Comment> commentRepository,
            IAsyncRepository<Community> communityRepository)
        {
            _replyRepository = replyRepository;
            _likeRepository = likeRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _communityRepository = communityRepository;
        }
        
        public async Task<Reply> CreateReply(CreateReplyDto input)
        {
            var reply = new Reply
            {
                Content = input.Content,
                UserId = input.UserId,
                CommentId = input.CommentId
            };
            if (input.ParentId != null)
            {
                reply.ParentId = input.ParentId;
            }
            await _replyRepository.AddAsync(reply);

            var user = await _userRepository.GetByIdAsync(input.UserId);
            var comment = await _commentRepository.GetByIdAsync(input.CommentId);
            var post = await _postRepository.GetByIdAsync(comment.PostId);
            var community = await _communityRepository.GetByIdAsync(post.CommunityId);

            if (comment.UserId == user.Id) return reply;
            var notify = new Notification
            {
                Content = user.Username + " " + "yorumuna yanıt verdi: " + input.Content,
                OwnerUserId = comment.UserId,
                Type = NotifyContentType.CommentReply,
                TargetName = community.Slug+"/"+post.Slug,
                ImgPath = user.ProfileImagePath,
                TargetId = post.Id
            };
            await _notificationRepository.AddAsync(notify);
            return reply;
        }

        public async Task<ReplyLike> Like(Guid userId, Guid replyId)
        {
            var isExist = await _likeRepository.GetAll().Where(x => x.UserId == userId && x.ReplyId == replyId && x.IsDeleted==false)
                .FirstOrDefaultAsync();
            if (isExist != null)
            {
                throw new Exception("Bu islem zaten yapilmis");
            }
            
            var model = new ReplyLike
            {
                UserId = userId,
                ReplyId = replyId
            };
            await _likeRepository.AddAsync(model);
            
            var user = await _userRepository.GetByIdAsync(userId);
            var reply = await _replyRepository.GetByIdAsync(replyId);
            var comment = await _commentRepository.GetByIdAsync(reply.CommentId);
            var post = await _postRepository.GetByIdAsync(comment.PostId);
            var community = await _communityRepository.GetByIdAsync(post.CommunityId);
            if (reply.UserId == user.Id) return model;
            var notify = new Notification
            {
                Content = user.Username + " " + "yanıtını beğendi " + reply.Content,
                OwnerUserId = reply.UserId,
                Type = NotifyContentType.ReplyLike,
                ImgPath = user.ProfileImagePath,
                TargetName = community.Slug+"/"+post.Slug ,
                TargetId = post.Id
            };
            await _notificationRepository.AddAsync(notify);
            
            return model;
        }

        public async Task Unlike(Guid userId, Guid replyId)
        {
            var like = await _likeRepository.GetAll().Where(x => x.UserId == userId && x.ReplyId == replyId && x.IsDeleted==false )
                .FirstOrDefaultAsync();
            like.IsDeleted = true;
            await _likeRepository.UpdateAsync(like);
        }
        
        public async Task Delete(Guid id)
        {
            var comment = await _replyRepository.GetByIdAsync(id);
            comment.IsDeleted = true;
            await _replyRepository.UpdateAsync(comment);
        }
    }
}