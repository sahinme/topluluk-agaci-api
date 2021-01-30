using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Nnn.ApplicationCore.Entities.Comments;
using Microsoft.Nnn.ApplicationCore.Entities.Communities;
using Microsoft.Nnn.ApplicationCore.Entities.PostTags;
using Microsoft.Nnn.ApplicationCore.Entities.PostVotes;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.Posts
{
    public class Post:BaseEntity,IAggregateRoot
    {
        public string Content { get; set; }
        public string MediaContentPath { get; set; }
        public string Slug { get; set; }
        public ContentType ContentType { get; set; }
        [DefaultValue(false)]
        public bool IsPinned { get; set; }
        public Guid UserId { get; set; }
        public Guid CommunityId { get; set; }
        public string LinkUrl { get; set; }
        
        public Community Community { get; set; } 
        public User User { get; set; }
        
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<PostVote> Votes { get; set; }
        public virtual ICollection<PostTag> Tags { get; set; }
    }
}