using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Nnn.ApplicationCore.Entities.Communities;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.ModeratorOperations
{
    public class ModeratorOperation:BaseEntity,IAggregateRoot
    {
        public string Operation { get; set; }
        public Guid ModeratorId { get; set; }
        public Guid CommunityId { get; set; }
        public Guid? PostId { get; set; }
        public Guid? UserId { get; set; }
        
        [ForeignKey(nameof(ModeratorId))]
        public virtual User Moderator { get; set; }
            
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
        
        [ForeignKey(nameof(CommunityId))]
        public virtual Community Community { get; set; }
        
        [ForeignKey(nameof(PostId))]
        public virtual Post Post { get; set; }
    }
}