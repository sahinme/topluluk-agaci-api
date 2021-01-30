using System;

namespace Microsoft.Nnn.ApplicationCore.Services.PostAppService.Dto
{
    public class CreateLikeDto
    {
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
    }
}