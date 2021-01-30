using System;

namespace Microsoft.Nnn.ApplicationCore.Services.CommunityService.Dto
{
    public class AddModeratorDto
    {
        public Guid UserId { get; set; }
        public Guid RequesterModeratorId { get; set; }
        public string CommunitySlug { get; set; }
    }
}