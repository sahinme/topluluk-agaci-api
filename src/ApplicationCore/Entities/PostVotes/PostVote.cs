using System;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.PostVotes
{
    public class PostVote:BaseEntity,IAggregateRoot
    {
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public sbyte Value { get; set; }

        public User User { get; set; }
        public Post Post { get; set; }
    }
}