using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.Communities;
using Microsoft.Nnn.ApplicationCore.Entities.CommunityUsers;
using Microsoft.Nnn.ApplicationCore.Entities.ModeratorOperations;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.BlobService;
using Microsoft.Nnn.ApplicationCore.Services.CommunityService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.Dto;
using Microsoft.Nnn.ApplicationCore.Services.PasswordHasher;
using Nnn.ApplicationCore.Services.UserService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.UserService
{
    public class UserService:IUserService
    {
        private readonly IAsyncRepository<User> _userRepository;
        private readonly IAsyncRepository<CommunityUser> _communityUserRepository;
        private readonly IAsyncRepository<Community> _communityRepository;
        private readonly IAsyncRepository<ModeratorOperation> _moderatorOperationRepository;
        private readonly IBlobService _blobService;
        private readonly IEmailSender _emailSender;

        public UserService(
            IAsyncRepository<User> userRepository,IBlobService blobService,
            IAsyncRepository<CommunityUser> communityUserRepository,
            IAsyncRepository<ModeratorOperation> moderatorOperationRepository,
            IAsyncRepository<Community> communityRepository,
            IEmailSender emailSender
            )
        {
            _userRepository = userRepository;
            _blobService = blobService;
            _communityUserRepository = communityUserRepository;
            _moderatorOperationRepository = moderatorOperationRepository;
            _communityRepository = communityRepository;
            _emailSender = emailSender;
        }
        
        public async Task<User> CreateUser(CreateUserDto input)
        {
            var isUsernameTaken = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.Username == input.Username);
            if (isUsernameTaken != null)
            {
                var model = new User();
                model.Username = "Bu kullanıcı adı daha önce alınmış";
                return model;
            }
            
            var isEmailTaken = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.EmailAddress == input.EmailAddress);
            if (isEmailTaken != null)
            {
                var model = new User();
                model.EmailAddress = "Bu E-Posta adresi daha önce alınmış";
                return model;
            }
        
            var user = new User
            {
                Username = input.Username,
                EmailAddress = input.EmailAddress,
                Gender = input.Gender,
                Bio = input.Bio,
                VerificationCode = RandomString.GenerateString(35)
            };
            if (input.ProfileImage!=null)
            {
                var imgPath = await _blobService.InsertFile(input.ProfileImage);
                user.ProfileImagePath = imgPath;
            }
            var hashedPassword = SecurePasswordHasherHelper.Hash(input.Password);
            user.Password = hashedPassword;
            await _userRepository.AddAsync(user);
            return user;
        }

        public async Task<UserDto> GetUserById(Guid id)
        {
            var user = await _userRepository.GetAll().Where(x => x.Id == id).Select(x => new UserDto
            {
                Id = x.Id,
                Username = x.Username,
                Bio = x.Bio,
                Gender = x.Gender,
                ProfileImagePath = x.ProfileImagePath == null ? null : BlobService.BlobService.GetImageUrl(x.ProfileImagePath)
            }).FirstOrDefaultAsync();
            return user;
        }
        
        public async Task<UserDto> GetByUsername(string username)
        {
            var user = await _userRepository.GetAll().Where(x => x.Username == username).Select(x => new UserDto
            {
                Id = x.Id,
                Username = x.Username,
                SPoint = x.SPoint,
                Bio = x.Bio,
                Gender = x.Gender,
                ProfileImagePath = x.ProfileImagePath == null ? null : BlobService.BlobService.GetImageUrl(x.ProfileImagePath)
            }).FirstOrDefaultAsync();

            var comMods = new List<string>();
            comMods = await _communityUserRepository.GetAll()
                .Where(x => x.UserId == user.Id && x.IsAdmin && x.IsDeleted == false)
                .Select(x=>x.Community.Slug)
                .ToListAsync();

            user.ComMods = comMods;
            
            var isModerator = await _communityUserRepository.GetAll().AnyAsync(x =>
                x.IsDeleted == false && x.Suspended == false && x.IsAdmin && x.UserId == user.Id);
            user.IsModerator = isModerator;
            return user;    
        }

        public async Task<bool> VerifyEmail(string verificationCode)
        {
            var user = await _userRepository.GetAll().Where(x => x.VerificationCode == verificationCode)
                .FirstOrDefaultAsync();
            if (user == null)
            {
                return false;
            }
            user.EmailVerified = true;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> SendResetCode(string emailAddress)
        {
            try
            {
                var user = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.EmailAddress == emailAddress);
                if(user==null) throw new Exception("Kullanıcı bulunamadı");

                var resetCode = RandomString.GenerateString(10);
                user.ResetPasswordCode = resetCode;
                await _userRepository.UpdateAsync(user);
                var message = "Şifre Sıfırlama İçin Doğrulama Kodu: " + resetCode;
                await _emailSender.SendEmail(emailAddress, "Şifre Sıfırlama", message);
                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }
    
        public async Task<bool> ResetPassword(ResetPasswordDto input)
        {
            var user = await _userRepository.GetAll().Where(x =>
                    x.EmailAddress == input.EmailAddress && x.ResetPasswordCode == input.ResetCode)
                .FirstOrDefaultAsync();
            if (user == null)
            {
                throw new Exception("Böyle bir işlem yok");
            }
            
            var hashedPassword = SecurePasswordHasherHelper.Hash(input.NewPassword);
            user.Password = hashedPassword;
            await _userRepository.UpdateAsync(user);
            return true;
        }
        
        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            var user = await _userRepository.GetByIdAsync(input.UserId);
            if (user == null)
            {
                throw new Exception("Kullanici bulunamadi");
            }
            var decodedPassword = SecurePasswordHasherHelper.Verify(input.OldPassword, user.Password);
            if(!decodedPassword) throw new Exception("Eski sifre yanlis");
            var hashedPassword = SecurePasswordHasherHelper.Hash(input.NewPassword);
            user.Password = hashedPassword;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<Response> HandleModeratorRequest(HandleModRequest input)
        {
            var response = new Response();
            
            var isExist = await _communityUserRepository.GetAll()
                .Where(x => x.Community.Slug == input.ComSlug && x.UserId == input.UserId &&
                            x.ModeratorRequest == ModeratorRequest.Waiting && !x.IsDeleted)
                .FirstOrDefaultAsync();
            
            if (isExist == null)
            {
                response.Message = "İstek bulunumadı";
                response.Status = false;
                return response;
            }
            
            isExist.ModeratorRequest = input.Value;
            if (input.Value == ModeratorRequest.Confirmed)
            {
                isExist.IsAdmin = true;
            }
            await _communityUserRepository.UpdateAsync(isExist);
            response.Message = "İşlem başarılı";
            response.Status = true;
            return response;
        }

        public async Task UpdateUser(UpdateUserDto input)
        {
            var user = await _userRepository.GetByIdAsync(input.Id);
            
            if (input.Username != null) user.Username = input.Username;
            if (input.EmailAddress != null) user.EmailAddress = input.EmailAddress;
            if (input.Bio != null) user.Bio = input.Bio;
            if (input.Username != null) user.Username = input.Username;
            user.Gender = input.Gender;
            
            if (input.ProfileImage != null)
            {
                var imgPath = await _blobService.InsertFile(input.ProfileImage);
                user.ProfileImagePath = imgPath;
            }
            await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> Login(LoginDto input)
        {
            var user = await _userRepository.GetAll()
                .FirstOrDefaultAsync(x => x.Username == input.Username);
            if (user == null)
            {
                return false;
            }
            var decodedPassword = SecurePasswordHasherHelper.Verify(input.Password, user.Password);
            return decodedPassword;
        }

        public async Task DeleteUser(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            user.IsDeleted = true;
            await _userRepository.UpdateAsync(user);
        }

        public async Task<CommunityUser> JoinCommunity(Guid userId, string slug)
        {
            var isExist = await _communityUserRepository.GetAll()
                .Where(x => x.IsDeleted == false && x.UserId==userId && x.Community.Slug == slug )
                .FirstOrDefaultAsync();
            if (isExist != null)
            {
                throw new Exception("bu islem zaten yapilmis");
            }

            var joinedUser = await _userRepository.GetByIdAsync(userId);
            var community = await _communityRepository.GetAll().FirstOrDefaultAsync(x => x.Slug == slug);
            var moderators = _communityUserRepository.GetAll().Include(x => x.User)
                .Where(x => x.CommunityId == community.Id && x.IsAdmin && x.IsDeleted==false);
            
            var model = new CommunityUser
            {
                UserId = userId,
                CommunityId = community.Id
            };
            await _communityUserRepository.AddAsync(model);
            foreach (var user in moderators)
            {
                await _emailSender.SendEmail(user.User.EmailAddress, "Kullanıcılar topluluğunu keşfediyor",
                    joinedUser.Username + " topluluğuna katıldı: https://saalla.com/t/"+community.Slug);
            }
            return model;
        }
        
        public async Task LeaveFromCommunity(Guid userId, string slug)
        {
            var isExist = await _communityUserRepository.GetAll()
                .Where(x => x.Community.Slug == slug && x.UserId == userId && x.IsDeleted == false )
                .FirstOrDefaultAsync();
            
            if (isExist == null) throw new Exception("this relation don`t exist");
            isExist.IsDeleted = true;
            await _communityUserRepository.UpdateAsync(isExist);
        }
        
        public async Task ModeratorRejectedJoin(ModeratorRejected input)
        {
            var isExist = await _communityUserRepository.GetAll()
                .Where(x => x.Community.Slug == input.Slug && x.User.Username == input.Username && x.IsDeleted == false )
                .FirstOrDefaultAsync();
            if (isExist == null) throw new Exception("this relation don`t exist");

            var community = await _communityRepository.GetAll()
                .FirstOrDefaultAsync(x => x.Slug == input.Slug && !x.IsDeleted);

            var user = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.Username == input.Username);
            
            isExist.IsDeleted = true;
            await _communityUserRepository.UpdateAsync(isExist);

            var model = new ModeratorOperation
            {
                Operation = "USER_REJECTED",
                CommunityId = community.Id,
                ModeratorId = input.ModeratorId,
                UserId = user.Id
            };
            await _moderatorOperationRepository.AddAsync(model);
        }
        
        public async Task<List<GetAllCommunityDto>> GetUserCommunities(Guid userId)
        {
            var result = await _communityUserRepository.GetAll()
                .Where(x => x.UserId == userId && x.IsDeleted == false && x.Suspended == false)
                .Include(x => x.Community).Select(x => new GetAllCommunityDto
                {
                    Slug = x.Community.Slug,
                    Name = x.Community.Name,
                    Description = x.Community.Description,
                    LogoPath = x.Community.LogoPath == null ? null : BlobService.BlobService.GetImageUrl(x.Community.LogoPath),
                    MemberCount = x.Community.Users.Count(m=>m.IsDeleted==false)
                }).ToListAsync();
            return result;
        }
    }
}