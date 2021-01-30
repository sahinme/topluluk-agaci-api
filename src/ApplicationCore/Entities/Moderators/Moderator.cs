using System;
using Microsoft.Nnn.ApplicationCore.Interfaces;

namespace Microsoft.Nnn.ApplicationCore.Entities.Moderators
{
    public class Moderator:BaseEntity,IAggregateRoot
    {
        public Guid UserId { get; set; }
        public Guid CommunityId { get; set; }
    }
}