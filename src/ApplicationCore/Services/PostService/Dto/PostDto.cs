using System;
using System.Collections.Generic;
using Microsoft.Nnn.ApplicationCore.Entities.Comments;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.PostVotes;
using Microsoft.Nnn.ApplicationCore.Services.CommentService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.PostAppService.Dto
{
    public class PostDto
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public string Content { get; set; }
        public string ContentPath { get; set; }
        public ContentType ContentType { get; set; }
        public string LinkUrl { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long VoteCount { get; set; }
        public PostVote UserPostVote { get; set; }
        public PostCommunityDto Community { get; set; }
        public PostUserDto UserInfo { get; set; }
        public List<CommentDto> Comments { get; set; }
       
    }

    public class PostUserDto
    {
        public string UserName { get; set; }
        public string ProfileImagePath { get; set; }
    }
}