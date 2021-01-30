using System;

namespace Microsoft.Nnn.ApplicationCore.Services.CommentService.Dto
{
    public class CreateCommentDto
    {
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public string Content { get; set; }
    }
}