using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.Categories;
using Microsoft.Nnn.ApplicationCore.Entities.Communities;
using Microsoft.Nnn.ApplicationCore.Entities.CommunityUsers;
using Microsoft.Nnn.ApplicationCore.Entities.Notifications;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.BlobService;
using Microsoft.Nnn.ApplicationCore.Services.CommunityService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.Dto;
using Microsoft.Nnn.ApplicationCore.Services.PostAppService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.PostService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.UserService;

namespace Microsoft.Nnn.ApplicationCore.Services.CommunityService
{
    public class CommunityAppService:ICommunityAppService
    {
        private readonly IAsyncRepository<Community> _communityRepository;
        private readonly IAsyncRepository<CommunityUser> _communityUserRepository;
        private readonly IAsyncRepository<Category> _categoryRepository;
        private readonly IAsyncRepository<Post> _postRepository;
        private readonly IAsyncRepository<Notification> _notificationRepository;
        private readonly IEmailSender _emailSender;
        private readonly IAsyncRepository<User> _userRepository;
        private readonly IBlobService _blobService;

        public CommunityAppService(IAsyncRepository<Community> communityRepository,IBlobService blobService,
            IAsyncRepository<Post> postRepository,IAsyncRepository<CommunityUser> communityUserRepository,
            IAsyncRepository<Notification> notificationRepository, IEmailSender emailSender,
            IAsyncRepository<User> userRepository,IAsyncRepository<Category> categoryRepository)
        {
            _communityRepository = communityRepository;
            _blobService = blobService;
            _postRepository = postRepository;
            _userRepository = userRepository;
            _communityUserRepository = communityUserRepository;
            _notificationRepository = notificationRepository;
            _categoryRepository = categoryRepository;
            _emailSender = emailSender;
        }
        
        public async Task<Response> CreateCommunity(CreateCommunity input)
        {
            
            
            var responseModel = new Response();
            
            var userComs = await _communityUserRepository.GetAll()
                .Where(x => x.UserId == input.UserId && x.IsAdmin && x.IsDeleted == false)
                .ToListAsync();
            var user = await _userRepository.GetByIdAsync(input.UserId);

            if (userComs.Count > 1 && !user.IsAdmin )
            {
                responseModel.Status = false;
                responseModel.Message = "Daha fazla topluluk oluşturamazsınız";
                return responseModel;
            }

            var slug = Slug.FriendlyUrlTitle(input.Name);

            var isExist =  _communityRepository.GetAll().Any(x => x.Slug == slug);
            if (isExist)
            {
                responseModel.Status = false;
                responseModel.Message = "Bu isimde bir topluluk zaten var";
                return responseModel;
            }

            var category = await _categoryRepository.GetAll().FirstOrDefaultAsync(x => x.Slug == input.CatSlug);
            var model = new Community
            {
                Name = input.Name,
                Description = input.Description,
                CategoryId = category.Id,
                Slug = slug
            };
            if (input.LogoFile != null)
            {
                var path = await _blobService.InsertFile(input.LogoFile);
                model.LogoPath = path;
            }
            if (input.CoverImage != null)
            {
                var path = await _blobService.InsertFile(input.CoverImage);
                model.CoverImagePath = path;
            }
            var result = await _communityRepository.AddAsync(model);
            var communityUser = new CommunityUser
            {
                CommunityId = result.Id,
                UserId = input.UserId,
                IsAdmin = true
            };
            await _communityUserRepository.AddAsync(communityUser);
            responseModel.Message = "Topluluk başarıyla oluşturuldu";
            responseModel.Status = true;
            responseModel.Slug = model.Slug;
            return responseModel;
        }

        public async Task<List<GetAllCommunityDto>> GetAll()
        {
            var result = await _communityRepository.GetAll().Where(x => x.IsDeleted == false)
                .Include(x => x.Users).ThenInclude(x => x.User).Select(x=> new GetAllCommunityDto
                {
                    Slug = x.Slug,
                    Name = x.Name,
                    Description = x.Description,
                    MemberCount = x.Users.Count(m=> !m.IsDeleted),
                    LogoPath = x.LogoPath == null ? null : BlobService.BlobService.GetImageUrl(x.LogoPath)
                }).ToListAsync();
            return result;
        }

        public async Task<Community> Update(UpdateCommunity input)
        {
            var isAdmin = await _communityUserRepository.GetAll()
                .FirstOrDefaultAsync(x =>
                    x.IsDeleted == false && x.IsAdmin && x.UserId == input.ModeratorId && x.Community.Slug == input.Slug);

            if (isAdmin == null)
            {
                throw new Exception("Bu kullanıcının yetkisi yok");
            }
            
            var community = await _communityRepository.GetByIdAsync(isAdmin.CommunityId);
            if (input.Name != null) community.Name = input.Name;
            if (input.Description != null) community.Description = input.Description;
            if (input.Logo != null)
            {
                var path = await _blobService.InsertFile(input.Logo);
                community.LogoPath = path;
            }
            if (input.CoverPhoto != null)
            {
                var path = await _blobService.InsertFile(input.CoverPhoto);
                community.CoverImagePath = path;
            }

            await _communityRepository.UpdateAsync(community);
            return community;
        }

        public async Task<List<GetAllCommunityDto>> OfModerators(Guid userId)
        {
            var result = await _communityUserRepository.GetAll()
                .Where(x => x.IsDeleted == false && x.IsAdmin && x.UserId == userId)
                .Include(x => x.User).Include(x => x.Community).ThenInclude(x => x.Users)
                .Select(x => new GetAllCommunityDto
                {
                    Slug = x.Community.Slug,
                    Description = x.Community.Description,
                    LogoPath = x.Community.LogoPath == null ? null : BlobService.BlobService.GetImageUrl(x.Community.LogoPath),
                    MemberCount = x.Community.Users.Count(m => m.IsDeleted == false),
                    Name = x.Community.Name
                }).ToListAsync();
            return result;
        }

        public async Task<List<CommunityUserDto>> Users(string slug)
        {
            var result = await _communityRepository.GetAll().Where(x => x.Slug == slug && x.IsDeleted == false)
                .Include(x => x.Users)
                .ThenInclude(x => x.User).Select(x => new CommunityDto
                {
                    Members = x.Users.Where(m=>m.IsDeleted==false).Select(m => new CommunityUserDto
                    {
                        UserId = m.User.Id,
                        PostCount = m.User.Posts.Count(p=>p.IsDeleted==false),
                        Username = m.User.Username,
                        ProfileImg = m.User.ProfileImagePath == null ? null : BlobService.BlobService.GetImageUrl(m.User.ProfileImagePath)
                    }).ToList()
                }).FirstOrDefaultAsync();
            var users = result.Members;
            return users;
        }

        public async Task<CommunityDto> GetById(string slug,Guid? userId)
        {
            var result = await _communityRepository.GetAll().Where(x => x.Slug == slug && x.IsDeleted == false)
                .Include(x => x.Users)
                .ThenInclude(x => x.User).Select(x => new CommunityDto
                {
                    Slug = x.Slug,
                    Name = x.Name,
                    Description = x.Description,
                    LogoPath = x.LogoPath == null ? null : BlobService.BlobService.GetImageUrl(x.LogoPath),
                    CoverImagePath = x.CoverImagePath == null ? null : BlobService.BlobService.GetImageUrl(x.CoverImagePath),
                    CreatedDate = x.CreatedDate,
                    Moderators = x.Users.Where(q=>q.IsDeleted==false && q.Suspended==false && q.IsAdmin ).Select(q=>new CommunityUserDto
                    {
                        Username = q.User.Username,
                        ProfileImg = BlobService.BlobService.GetImageUrl(q.User.ProfileImagePath)
                    }).ToList(),
                    Members = x.Users.Where(m=>m.IsDeleted==false).Select(m => new CommunityUserDto
                    {
                        Username = m.User.Username,
                        ProfileImg = BlobService.BlobService.GetImageUrl(m.User.ProfileImagePath)
                    }).ToList()
                }).FirstOrDefaultAsync();
            return result;
        }

        public async Task<PagedResultDto<CommunityPostDto>> GetPosts(PageDtoCommunity input)
        {
            var posts = await _postRepository.GetAll().Where(x => x.IsDeleted==false && !x.IsPinned && x.Community.Slug == input.Slug)
                .Include(x => x.Comments).ThenInclude(x => x.Replies)
                .Include(x => x.User).Select(x => new CommunityPostDto
                {
                    Id = x.Id,
                    Slug = x.Slug,
                    Content = x.Content,
                    PageNumber = input.PageNumber,
                    LinkUrl = x.LinkUrl,
                    MediaContentPath = x.MediaContentPath == null ? null : BlobService.BlobService.GetImageUrl(x.MediaContentPath),
                    ContentType = x.ContentType,
                    CreatedDateTime = x.CreatedDate,
                    UserPostVote = x.Votes.FirstOrDefault(p=>p.IsDeleted==false &&
                                                             p.UserId == input.UserId  && p.PostId==x.Id ),
                    VoteCount = x.Votes.Count(v=>v.IsDeleted==false && v.Value==1) - x.Votes.Count(v=>v.IsDeleted==false && v.Value==-1),
                    Comments = x.Comments.Where(f=>f.IsDeleted==false).Select(f=> new Comment
                    {
                        ReplyCount = f.Replies.Count(v=>v.IsDeleted==false)
                    }).ToList(),
                    User = new PostUserDto
                    {
                        ProfileImagePath = BlobService.BlobService.GetImageUrl(x.User.ProfileImagePath),
                        UserName = x.User.Username
                    },
                }).OrderByDescending(x=>x.CreatedDateTime).Skip((input.PageNumber - 1) * input.PageSize).Take(input.PageSize).ToListAsync();
            var hasNext = await _postRepository.GetAll().Where(x => x.IsDeleted==false && x.Community.Slug == input.Slug)
                .Skip((input.PageNumber) * input.PageSize).AnyAsync();
            var pinnedPosts = await GetPinnedPosts(input);
            var totalResult = posts.Concat(pinnedPosts);
            var bb = new PagedResultDto<CommunityPostDto> {Results = totalResult ,  HasNext = hasNext};
            return bb;
        }
        
        public async Task<List<CommunityPostDto>> GetPinnedPosts(PageDtoCommunity input)
        {
            var posts = await _postRepository.GetAll().Where(x => x.IsDeleted==false && x.IsPinned && x.Community.Slug == input.Slug)
                .Include(x => x.Comments).ThenInclude(x => x.Replies)
                .Include(x => x.User).Select(x => new CommunityPostDto
                {
                    Id = x.Id,
                    IsPinned = x.IsPinned,
                    Slug = x.Slug,
                    Content = x.Content,
                    PageNumber = input.PageNumber,
                    LinkUrl = x.LinkUrl,
                    MediaContentPath = x.MediaContentPath == null ? null : BlobService.BlobService.GetImageUrl(x.MediaContentPath),
                    ContentType = x.ContentType,
                    CreatedDateTime = x.CreatedDate,
                    UserPostVote = x.Votes.FirstOrDefault(p=>p.IsDeleted==false &&
                                                             p.UserId == input.UserId  && p.PostId==x.Id ),
                    VoteCount = x.Votes.Count(v=>v.IsDeleted==false && v.Value==1) - x.Votes.Count(v=>v.IsDeleted==false && v.Value==-1),
                    Comments = x.Comments.Where(f=>f.IsDeleted==false).Select(f=> new Comment
                    {
                        ReplyCount = f.Replies.Count(v=>v.IsDeleted==false)
                    }).ToList(),
                    User = new PostUserDto
                    {
                        ProfileImagePath = BlobService.BlobService.GetImageUrl(x.User.ProfileImagePath),
                        UserName = x.User.Username
                    },
                }).OrderByDescending(x=>x.CreatedDateTime).ToListAsync();
            return posts;
        }

        public async Task<List<GetAllCommunityDto>> GetPopulars(Guid? userId)
        {
            var result = await _communityRepository.GetAll().Where(x => x.IsDeleted == false)
                .Include(x => x.Users).ThenInclude(x => x.User)
                .Select(x=> new GetAllCommunityDto
                {
                    Slug = x.Slug,
                    Name = x.Name,
                    Description = x.Description,
                    MemberCount = x.Users.Count(m=>m.IsDeleted==false),
                    LogoPath = x.LogoPath == null ? null : BlobService.BlobService.GetImageUrl(x.LogoPath),
                    IsUserJoined = x.Users.Any(u=>u.IsDeleted==false && u.UserId==userId )
                }).OrderByDescending(x=>x.MemberCount).Take(5).ToListAsync();
            return result;

        }
        
        public async Task<List<SearchDto>> Search(string text)
        {
            var coms = await _communityRepository.GetAll().Where(x =>
                    x.IsDeleted == false && x.Name.Contains(text) || x.Description.Contains(text))
                .Select(x => new SearchDto
                {
                    Name = x.Slug,
                    LogoPath = x.LogoPath == null ? null : BlobService.BlobService.GetImageUrl(x.LogoPath),
                    MemberCount = x.Users.Count(c => !c.IsDeleted),
                    Type = "community"
                }).ToListAsync();

            var users = await _userRepository.GetAll().Where(x =>
                    x.IsDeleted == false && x.Username.Contains(text) || x.EmailAddress.Contains(text))
                .Select(x => new SearchDto
                {
                    LogoPath = x.ProfileImagePath == null ? null : BlobService.BlobService.GetImageUrl(x.ProfileImagePath),
                    Name = x.Username,
                    Type = "user"
                }).ToListAsync();

            var result = coms.Union(users).ToList();
            return result;
        }

        public async Task<Response> AddModerator(AddModeratorDto input)
        {
            var response = new Response();
            var community = await _communityRepository.GetAll()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Slug == input.CommunitySlug);
            var requester = await _userRepository.GetByIdAsync(input.RequesterModeratorId);
            var user = await _userRepository.GetByIdAsync(input.UserId);
         
            var isAlreadyExist = await _communityUserRepository.GetAll()
                .Where(x => x.UserId == input.UserId && x.Community.Slug == input.CommunitySlug &&
                            x.IsDeleted == false && x.IsAdmin).FirstOrDefaultAsync();
            if (isAlreadyExist != null)
            {
                response.Message = "Bu kullanıcı zaten moderatör";
                response.Status = false;
                return response;
            }

            var isWaiting = await  _communityUserRepository.GetAll()
                .Where(x => x.UserId == input.UserId && x.Community.Slug == input.CommunitySlug &&
                            !x.IsDeleted && x.ModeratorRequest == ModeratorRequest.Waiting && !x.IsAdmin).FirstOrDefaultAsync();
            if (isWaiting != null)
            {
                response.Message = "Bu kullanıcıya zaten istek gönderilmiş";
                response.Status = false;
                return response;
            }

            var isJoined = await _communityUserRepository.GetAll()
                    .Where(x => x.UserId == input.UserId && x.Community.Slug == input.CommunitySlug && !x.IsDeleted)
                    .FirstOrDefaultAsync();
                if (isJoined != null)
                {
                    isJoined.ModeratorRequest = ModeratorRequest.Waiting;
                    await _communityUserRepository.UpdateAsync(isJoined);

                    response.Message = "Kullanıcıya istek gönderildi.";
                    response.Status = true;
                    var content = requester.Username + " " + "seni" + " " + community.Name +
                                  " topluluğuna moderatör olarak eklemek istiyor.";
                    var notify = new Notification
                    {
                        TargetId = community.Id,
                        OwnerUserId = input.UserId,
                        TargetName = "/t/"+community.Slug,
                        Type = NotifyContentType.AddModerator,
                        Content = content,
                        ImgPath = requester.ProfileImagePath
                    };
                    await _notificationRepository.AddAsync(notify);
                    await _emailSender.SendEmail(user.EmailAddress, "Moderatörlük isteği", content+ " https://saalla.com");
                    return response;
                }

                var newRelation = new CommunityUser
                {
                    UserId = input.UserId,
                    CommunityId = community.Id,
                    ModeratorRequest = ModeratorRequest.Waiting
                };
                await _communityUserRepository.AddAsync(newRelation);
                response.Message = "Kullanıcıya istek gönderildi.";
                response.Status = true;
                var content2 = requester.Username + " seni" + community.Name +
                               " topluluğuna moderatör olarak eklemek istiyor.";
                var notify2 = new Notification
                {
                    TargetId = community.Id,
                    OwnerUserId = input.UserId,
                    TargetName = "/t/"+community.Slug,
                    Type = NotifyContentType.AddModerator,
                    Content = requester.Username + " seni" + community.Name + " topluluğuna moderatör olarak eklemek istiyor.",
                    ImgPath = community.LogoPath
                };
                await _notificationRepository.AddAsync(notify2);
                await _emailSender.SendEmail(user.EmailAddress, "Moderatörlük isteği", content2+ " https://saalla.com");
                return response;
        }
    }
}