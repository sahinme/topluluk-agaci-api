using System;

namespace Microsoft.Nnn.ApplicationCore.Services.ReplyService.Dto
{
    public class ReplyDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public bool IsLoggedReply { get; set; }
        public ParentDto Parent { get; set; }
        public bool IsLoggedLiked { get; set; }
        public ReplyUserDto ReplyUserInfo { get; set; }
        public long LikeCount { get; set; }
    }

    public class ReplyUserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string ProfileImagePath { get; set; }
    }

    public class ParentDto
    {
        public string ParentReplyUserName { get; set; }
        public Guid? UserId { get; set; }
    }
}