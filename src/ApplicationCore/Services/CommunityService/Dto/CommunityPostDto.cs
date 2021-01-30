using System;
using System.Collections.Generic;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.PostVotes;
using Microsoft.Nnn.ApplicationCore.Services.PostAppService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.PostService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.CommunityService.Dto
{
    public class CommunityPostDto
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public string Content { get; set; }
        public string MediaContentPath { get; set; }
        public ContentType ContentType { get; set; }
        public int PageNumber { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public bool IsPinned { get; set; }
        public string LinkUrl { get; set; }
        public List<Comment> Comments { get; set; }
        public PostVote UserPostVote { get; set; }
        public long VoteCount { get; set; }
        public PostUserDto User { get; set; }
    }
}