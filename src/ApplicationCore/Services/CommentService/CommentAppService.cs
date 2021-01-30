using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.CommentLikes;
using Microsoft.Nnn.ApplicationCore.Entities.Comments;
using Microsoft.Nnn.ApplicationCore.Entities.Communities;
using Microsoft.Nnn.ApplicationCore.Entities.Notifications;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.CommentService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.ReplyService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.CommentService
{
    public class CommentAppService:ICommentAppService
    {
        private readonly IAsyncRepository<Comment> _commentRepository;
        private readonly IAsyncRepository<CommentLike> _commentLikeRepository;
        private readonly IAsyncRepository<Notification> _notificationRepository;
        private readonly IAsyncRepository<Community> _communityRepository;
        private readonly IAsyncRepository<User> _userRepository;
        private readonly IAsyncRepository<Post> _postRepository;
        private readonly IEmailSender _emailSender;
        public CommentAppService(IAsyncRepository<Comment> commentRepository,IAsyncRepository<CommentLike> commentLikeRepository,
            IAsyncRepository<Notification> notificationRepository,IAsyncRepository<User> userRepository,
            IAsyncRepository<Post> postRepository,IAsyncRepository<Community> communityRepository,
            IEmailSender emailSender)
        {
            _commentRepository = commentRepository;
            _commentLikeRepository = commentLikeRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _communityRepository = communityRepository;
            _emailSender = emailSender;
        }
        
        public async Task<Comment> CreateComment(CreateCommentDto input)
        {
            var comment = new Comment
            {
                Content = input.Content,
                UserId = input.UserId,
                PostId = input.PostId
            };
           await _commentRepository.AddAsync(comment);

           // notify
           var user = await _userRepository.GetByIdAsync(input.UserId);
           var post = await _postRepository.GetAll().Include(x=>x.User)
               .Where(x => x.IsDeleted == false && x.Id == input.PostId)
               .Include(x => x.Comments).FirstOrDefaultAsync();
           var community = await _communityRepository.GetByIdAsync(post.CommunityId);

           if (post.UserId == user.Id) return comment;
           var notify = new Notification
           {
               Content = user.Username + " " + "sallamanı yanıtladı : " + input.Content,
               ImgPath = user.ProfileImagePath,
               Type = NotifyContentType.PostComment,
               TargetId = post.Id,
               TargetName = community.Slug+"/"+post.Slug,
               OwnerUserId = post.UserId
           };
           await _notificationRepository.AddAsync(notify);
            // email send
            //var commentCount =  post.Comments.Count(c=>!c.IsDeleted);
            //if (commentCount != 0 && commentCount != 20 && commentCount != 50 && commentCount != 100) return comment;
            var url = "https://saalla.com/" + community.Slug + "/" + post.Slug;
            var message = user.Username + " kişisi gönderine salladı :"+url;
            var subject = "Gönderine sallıyorlar";
            await _emailSender.SendEmail(post.User.EmailAddress, subject, message);
            return comment;
        }

        public async Task<List<CommentDto>> GetPostComments(Guid postId)
        {
            var postComments = await _commentRepository.GetAll().Where(x => x.IsDeleted == false && x.PostId == postId)
                .Include(x => x.Post).Include(x => x.User)
                .Include(x=>x.Replies).ThenInclude(x=>x.User)
                .Select(x => new CommentDto
                {
                    Id = x.Id,
                    Content = x.Content,
                    CreatedDateTime = x.CreatedDate,
                    CommentUserInfo = new CommentUserDto
                    {
                        Id = x.User.Id,
                        UserName = x.User.Username,
                        ProfileImagePath = x.User.ProfileImagePath
                    },
                    Replies = x.Replies.Where(r=>r.IsDeleted==false).Select(r => new ReplyDto
                    {
                        Id = r.Id,
                        Content = r.Content,
                        CreatedDateTime = r.CreatedDate,
                        ReplyUserInfo = new ReplyUserDto
                        {
                            Id = r.User.Id,
                            UserName = r.User.Username,
                            ProfileImagePath = r.User.ProfileImagePath
                        }
                    }).ToList()
                }).ToListAsync();
            return postComments;
        }

        public async Task<Comment> UpdateComment(UpdateComment input)
        {
            var comment = await _commentRepository.GetByIdAsync(input.Id);
            comment.Content = input.Content;
            await _commentRepository.UpdateAsync(comment);
            return comment;
        }

        public async Task Delete(Guid id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            comment.IsDeleted = true;
            await _commentRepository.UpdateAsync(comment);
        }

        public async Task<CommentLike> Like(Guid userId, Guid commentId)
        {
            var isExist = await _commentLikeRepository.GetAll()
                .FirstOrDefaultAsync(x => x.IsDeleted==false && x.UserId == userId && x.CommentId == commentId);
            
            if (isExist != null)
            { 
                throw  new Exception("Bu islem daha once yapilmis");
            }

            var model = new CommentLike
            {
                UserId = userId,
                CommentId = commentId
            };
            await _commentLikeRepository.AddAsync(model);

            var comment = await _commentRepository.GetByIdAsync(commentId);
            var post = await _postRepository.GetByIdAsync(comment.PostId);
            var user = await _userRepository.GetByIdAsync(userId);
            var community = await _communityRepository.GetByIdAsync(post.CommunityId);
            if (comment.UserId == user.Id) return model;
            var notify = new Notification
            {
                Content = user.Username + " " + "sallamanı beğendi : " + comment.Content,
                TargetId = post.Id,
                ImgPath = user.ProfileImagePath,
                TargetName = community.Slug+"/"+post.Slug,
                OwnerUserId = comment.UserId,
                Type = NotifyContentType.CommentLike
            };
            await _notificationRepository.AddAsync(notify);
            
            return model;
        }
        
        public async Task Unlike(Guid userId, Guid commentId)
        {
            var isExist = await _commentLikeRepository.GetAll()
                .FirstOrDefaultAsync(x => x.IsDeleted==false && x.UserId == userId && x.CommentId == commentId);
            
            if (isExist == null)
            {
                throw  new Exception("Boyle bir islem yok");
            }

            isExist.IsDeleted = true;
            await _commentLikeRepository.UpdateAsync(isExist);

        }
    }
}