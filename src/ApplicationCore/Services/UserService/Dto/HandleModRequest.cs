using System;
using Microsoft.Nnn.ApplicationCore.Entities.CommunityUsers;

namespace Nnn.ApplicationCore.Services.UserService.Dto
{
    public class HandleModRequest
    {
        public string ComSlug { get; set; }
        public Guid UserId { get; set; }
        public ModeratorRequest Value { get; set; }
    }
}