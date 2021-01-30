using System;

namespace Nnn.ApplicationCore.Services.UserService.Dto
{
    public class ChangePasswordDto
    {
        public Guid UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}