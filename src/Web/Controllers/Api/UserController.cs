using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.Nnn.ApplicationCore.Entities.CommunityUsers;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.SuggestionService;
using Microsoft.Nnn.ApplicationCore.Services.SuggestionService.Dto;
using Microsoft.Nnn.Web.Identity;
using Nnn.ApplicationCore.Services.UserService.Dto;

namespace Microsoft.Nnn.Web.Controllers.Api
{
    public class UserController:BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;
        private readonly ISuggestionAppService _suggestionAppService;
        private readonly IAsyncRepository<CommunityUser> _communityUserRepository;
        private readonly IAsyncRepository<User> _userRepository;

        public UserController(IUserService userService,
                IEmailSender emailSender,ISuggestionAppService suggestionAppService,IAsyncRepository<CommunityUser> communityUserRepository,
                IAsyncRepository<User> userRepository
            )
        {
            _userService = userService;
            _emailSender = emailSender;
            _suggestionAppService = suggestionAppService;
            _communityUserRepository = communityUserRepository;
            _userRepository = userRepository;
        }

        [HttpGet("by-id")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var result = await _userService.GetUserById(id);
            return Ok(result);
        }
        
        [HttpGet]
        public async Task<IActionResult> Get(string username)
        {
            var result = await _userService.GetByUsername(username);
            return Ok(result);
        }
        
        [HttpPost]
        public async Task<IActionResult> Suggestion(SuggestionCreate input)
        {
            var result = await _suggestionAppService.Create(input);
            return Ok(result);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm]  CreateUserDto input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var user = await _userService.CreateUser(input);
                if (user.Id != Guid.Empty)
                {
                    var subject = "E-Postanızı onaylamak için linke tıklayın: https://saalla.com/verify/" + user.VerificationCode;
                    await _emailSender.SendEmail(user.EmailAddress, "E-posta onaylama", subject);
                }

                user.Password = null;
                user.VerificationCode = null;
                if (user.Id == Guid.Empty)
                    return Ok(new
                        {user.EmailAddress,user.Username, error = true});
                
                return Ok( new {success=true});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Ok(new {message = e});
                throw;
            }
           
        }
        
        
        [HttpPost]
        public async Task<IActionResult> SendMail(string email,string subject,string message)
        {
            await _emailSender.SendEmail(email, subject, message);
            return Ok();

        }
        
        
        [HttpPost]
        public async Task<IActionResult> HandleModReq(HandleModRequest input)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");
            
            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }
            
            input.UserId = Guid.Parse(userId);

            var result = await _userService.HandleModeratorRequest(input);
            return Ok(result);

        }
        
//        [HttpPost]
//        public async Task<IActionResult> Custom()
//        {
//            string url = "<a href='" + "https://saalla.com/" + "'>" + "Haydi hep beraber sallıyoruz" + "</a>";
//
//            string htmlString = @"<html>
//                      <body>
//                      <p>Saalla beta 1.1.2 yayında !</p>
//                      <p><strong>Saalla Beta güncellendi</strong></p><ul><li>SP (sallama puanı).</li><li>Moderatör kullanıcıları için gönderi sabitleme özelliği.</li><li>Topluluk moderatör ekleme özelliği.</li></ul><p>sallama puanını yükselterek ileri süreçte platform içinde açılacak yarışmalara katılma şansı yakala.</p><p>Yönettiğin toplulukta başta gözükmesini istediğin gönderini sabitle.</p><p>Moderatör ekip arkadaşı arıyorsan saalla kullanıcılarını moderatör olarak ekle.</p>
//                      <p>Destek ve sorularınız için,<br>destek@saalla.com</br></p>
//                      </body>
//                      </html>
//                     " + url ;
//            var users = await _userRepository.GetAll().Where(x => x.IsDeleted == false).ToListAsync();
//            foreach (var user in users)
//            {
//                await _emailSender.SendEmail(user.EmailAddress, "Saalla beta 1.1.2 yayında !", htmlString);
//            }
//            return Ok();
//
//        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> JoinCommunity(string slug)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            
            var result = await _userService.JoinCommunity(Guid.Parse(userId), slug);
            return Ok(result);
        }
        
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserCommunities()
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");
            
            var result = await _userService.GetUserCommunities(Guid.Parse(userId));
            return Ok(result);
        }
        
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> LeaveFromCommunity(string slug)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");
            
            await _userService.LeaveFromCommunity(Guid.Parse(userId), slug);
            return Ok();
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> ModeratorRejectedJoin(ModeratorRejected input)
        {
            var token = GetToken();
            if (!String.IsNullOrEmpty(token))
            {
                 var loggedUserId = LoginHelper.GetClaim(token, "UserId");
                 input.ModeratorId = Guid.Parse(loggedUserId);
            }

            var isAdmin = await _communityUserRepository.GetAll()
                .FirstOrDefaultAsync(x =>
                    x.IsDeleted == false && x.IsAdmin && x.UserId == input.ModeratorId && x.Community.Slug == input.Slug);
            if (isAdmin == null) return Unauthorized();
            
            await _userService.ModeratorRejectedJoin(input);
            return Ok();
        }
        
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserDto input)
        {
            var token = GetToken();
            var userId = LoginHelper.GetClaim(token, "UserId");

            if (string.IsNullOrEmpty(token)) return Unauthorized();
            input.Id = Guid.Parse(userId);
            await _userService.UpdateUser(input);
            return Ok();
        }

//        [Authorize]
//        [HttpDelete]
//        public async Task<IActionResult> DeleteUser(Guid id)
//        {
//            await _userService.DeleteUser(id);
//            return Ok();
//        }

//        [HttpPost]
//        public async Task EmailSend(string email, string subject, string message)
//        {
//            await _emailSender.SendEmail(email, subject, message);
//        }
    }
}