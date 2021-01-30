using System;

namespace Microsoft.Nnn.ApplicationCore.Services.ReplyService.Dto
{
    public class CreateReplyDto
    {
        public string Content { get; set; }
        public Guid UserId { get; set; }
        public Guid CommentId { get; set; }
        public Guid? ParentId { get; set; }
    }
}