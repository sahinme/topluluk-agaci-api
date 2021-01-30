using System;
using System.ComponentModel;
using Microsoft.Nnn.ApplicationCore.Entities.Communities;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.CommunityUsers
{
    public class CommunityUser:BaseEntity,IAggregateRoot
    {
        public Guid UserId { get; set; }
        public Guid CommunityId { get; set; }
        public bool IsAdmin { get; set; }
        
        [DefaultValue(false)]
        public bool Suspended { get; set; }
        
        public ModeratorRequest ModeratorRequest { get; set; }
        
        public User User { get; set; }
        public Community Community { get; set; }
    }
}