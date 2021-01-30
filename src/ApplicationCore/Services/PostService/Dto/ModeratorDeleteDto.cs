using System;

namespace Microsoft.Nnn.ApplicationCore.Services.PostService.Dto
{
    public class ModeratorDeleteDto
    {
        public Guid PostId { get; set; }
        public string Slug { get; set; }
        public Guid ModeratorId { get; set; }
    }
}