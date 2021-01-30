using System;
using Microsoft.AspNetCore.Http;

namespace Nnn.ApplicationCore.Services.UserService.Dto
{
    public class UpdateUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public IFormFile ProfileImage { get; set; }
        public string Bio { get; set; }
        public char Gender { get; set; }
    }
}