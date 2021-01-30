using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Nnn.ApplicationCore.Entities.CommentLikes;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.Replies;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.Comments
{
    public class Comment:BaseEntity,IAggregateRoot
    {
        public string Content { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }    
        public User User { get; set; }
        public Post Post { get; set; }
        public virtual ICollection<Reply> Replies { get; set; }
        public virtual ICollection<CommentLike> Likes { get; set; }
    }
}