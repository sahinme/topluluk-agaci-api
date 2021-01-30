using System;

namespace Microsoft.Nnn.ApplicationCore.Services.CommentService.Dto
{
    public class UpdateComment
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
    }
}