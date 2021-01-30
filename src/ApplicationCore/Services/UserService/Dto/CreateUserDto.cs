using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nnn.ApplicationCore.Services.UserService.Dto
{
    public class CreateUserDto
    {
        public char Gender { get; set; }
        
        [Required]
        [MinLength(4)]
        public string Username { get; set; }
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        
        [EmailAddress(ErrorMessage = "Gecersiz e-posta adresi")]
        public string EmailAddress { get; set; }
        public IFormFile ProfileImage { get; set; }
        
        [MaxLength(181)]
        public string Bio { get; set; }
        
       
    }
}