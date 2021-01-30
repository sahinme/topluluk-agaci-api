using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Nnn.ApplicationCore.Services.UserService.Dto;

namespace Microsoft.Nnn.Web.Controllers.Api
{
    public class AuthController:BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IAsyncRepository<User> _userRepository;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService,IConfiguration configuration,IAsyncRepository<User> userRepository)
        {
            _userService = userService;
            _configuration = configuration;
            _userRepository = userRepository;
        }
        
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto input )
        {
            if (!ModelState.IsValid) return BadRequest();
            var isUserValid = await _userService.Login(input); 
            if (!isUserValid)
            {
                return NotFound();
            }
            var userData = await _userService.GetByUsername(input.Username);
            var user = await _userRepository.GetByIdAsync(userData.Id);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, input.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("UserRole",user.IsAdmin ? "Admin" : "User"), 
            };

            var token = new JwtSecurityToken
            (
                issuer: _configuration["Issuer"], 
                audience: _configuration["Audience"],
                claims: claims,
                //expires: DateTime.UtcNow.AddDays(30), // 30 gün geçerli olacak
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SigningKey"])),//appsettings.json içerisinde bulunan signingkey değeri
                    SecurityAlgorithms.HmacSha256)
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token),success=true,user=userData });
        }

        [HttpPost]
        public async Task<IActionResult> SendResetCode(string emailAddress)
        {
            var result = await _userService.SendResetCode(emailAddress);
            return Ok(new {status = result});
        }
        
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto input)
        {
            var result = await _userService.ResetPassword(input);
            return Ok(new {status = result});
        }
        
        [HttpPut]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto input)
        {
            var result = await _userService.ChangePassword(input);
            return Ok(new {status = result});
        }
        
        [HttpPost]
        public async Task<IActionResult> Verify(string code)
        {
            var result = await _userService.VerifyEmail(code);
            return Ok(new {status = result});
        }
        
    }
}