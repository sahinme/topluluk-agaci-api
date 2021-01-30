using System;

namespace Nnn.ApplicationCore.Services.UserService.Dto
{
    public class ModeratorRejected
    {
        public string Username { get; set; }
        public string Slug { get; set; }
        public Guid ModeratorId { get; set; }
    }
}