using System;
using System.Collections.Generic;
using Microsoft.Nnn.ApplicationCore.Services.CategoryService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.PostAppService.Dto;
using Nnn.ApplicationCore.Services.UserService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.CommunityService.Dto
{
    public class CommunityDto
    {
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LogoPath { get; set; }
        public string CoverImagePath { get; set; }
        public List<CommunityUserDto> Moderators { get; set; }
        public List<CommunityUserDto> Members { get; set; }
        
    }

    public class CommunityUserDto    
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string ProfileImg { get; set; }
        public long PostCount { get; set; }
    }
}