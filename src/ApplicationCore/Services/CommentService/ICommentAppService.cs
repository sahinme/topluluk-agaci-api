using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Nnn.ApplicationCore.Entities.CommentLikes;
using Microsoft.Nnn.ApplicationCore.Entities.Comments;
using Microsoft.Nnn.ApplicationCore.Services.CommentService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.CommentService
{
    public interface ICommentAppService
    {
        Task<Comment> CreateComment(CreateCommentDto input);
        Task<List<CommentDto>> GetPostComments(Guid postId);
        Task<Comment> UpdateComment(UpdateComment input);
        Task Delete(Guid id);
        Task<CommentLike> Like(Guid userId,Guid commentId);
        Task Unlike(Guid userId, Guid commentId);
    }
}