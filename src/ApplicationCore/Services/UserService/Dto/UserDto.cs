using System;
using System.Collections.Generic;

namespace Nnn.ApplicationCore.Services.UserService.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string ProfileImagePath { get; set; }
        public char Gender { get; set; }
        public long SPoint { get; set; }
        public bool IsModerator { get; set; }
        public string Bio { get; set; }
        public List<string> ComMods { get; set; }
    }
}