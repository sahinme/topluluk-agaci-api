using System;
using Microsoft.Nnn.ApplicationCore.Entities.Comments;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.CommentLikes
{
    public class CommentLike:BaseEntity,IAggregateRoot
    {
        public Guid UserId { get; set; }
        public Guid CommentId { get; set; }

        public User User { get; set; }
        public Comment Comment { get; set; }
    }
}