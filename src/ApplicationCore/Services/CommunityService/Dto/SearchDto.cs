using System;

namespace Microsoft.Nnn.ApplicationCore.Services.CommunityService.Dto
{
    public class SearchDto
    {
        public string Name { get; set; }
        public string LogoPath { get; set; }
        public string Type { get; set; }
        public long MemberCount { get; set; }
    }
}