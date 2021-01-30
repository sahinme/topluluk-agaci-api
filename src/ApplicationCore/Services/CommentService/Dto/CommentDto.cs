using System;
using System.Collections.Generic;
using Microsoft.Nnn.ApplicationCore.Services.ReplyService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.CommentService.Dto
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Content { get; set; }
        public bool IsLoggedComment { get; set; }
        public bool IsLoggedLiked { get; set; }
        public long LikeCount { get; set; }
        public CommentUserDto CommentUserInfo { get; set; }
        public List<ReplyDto> Replies { get; set; }
    }

    public class CommentUserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string ProfileImagePath { get; set; }
    }
}