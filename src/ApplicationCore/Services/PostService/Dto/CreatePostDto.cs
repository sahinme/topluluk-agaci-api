using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;

namespace Microsoft.Nnn.ApplicationCore.Services.PostAppService.Dto
{
    public class CreatePostDto
    {
        public string Content { get; set; }
        public IFormFile ContentFile { get; set; }
        public string CommunitySlug { get; set; }
        public Guid UserId { get; set; }
        public ContentType ContentType { get; set; }

        public string LinkUrl { get; set; }
        //public string[] Tags { get; set; }
    }
}