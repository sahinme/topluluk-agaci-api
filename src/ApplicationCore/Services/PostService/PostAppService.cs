using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.Communities;
using Microsoft.Nnn.ApplicationCore.Entities.CommunityUsers;
using Microsoft.Nnn.ApplicationCore.Entities.ModeratorOperations;
using Microsoft.Nnn.ApplicationCore.Entities.Notifications;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.PostVotes;
using Microsoft.Nnn.ApplicationCore.Entities.Users;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.BlobService;
using Microsoft.Nnn.ApplicationCore.Services.CommentService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.Dto;
using Microsoft.Nnn.ApplicationCore.Services.PostAppService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.PostService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.ReplyService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.UserService;

namespace Microsoft.Nnn.ApplicationCore.Services.PostService
{
    public class PostAppService:IPostAppService
    {
        private readonly IAsyncRepository<Post> _postRepository;
        private readonly IAsyncRepository<PostVote> _postVoteRepository;
        private readonly IAsyncRepository<CommunityUser> _communityUserRepository;
        private readonly IAsyncRepository<ModeratorOperation> _moderatorOperationRepository;
        private readonly IAsyncRepository<User> _userRepository;
        private readonly IAsyncRepository<Notification> _notificationRepository;
        private readonly IEmailSender _emailSender;
        private readonly IBlobService _blobService;
        private readonly IAsyncRepository<Community> _communityRepository;

        public PostAppService(IAsyncRepository<Post> postRepository, 
            IBlobService blobService,
           IAsyncRepository<CommunityUser> communityUserRepository,
            IAsyncRepository<Community> communityRepository,
            IAsyncRepository<PostVote> postVoteRepository,
            IAsyncRepository<ModeratorOperation> moderatorOperationRepository,
            IAsyncRepository<Notification> notificationRepository,
            IEmailSender emailSender,
            IAsyncRepository<User> userRepository)
        {
            _postRepository = postRepository;
            _blobService = blobService;
            _communityUserRepository = communityUserRepository;
            _postVoteRepository = postVoteRepository;
            _moderatorOperationRepository = moderatorOperationRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _communityRepository = communityRepository;
            _emailSender = emailSender;
        }
        
        public async Task<Post> CreatePost(CreatePostDto input)
        {
            var com = await _communityRepository.GetAll().FirstOrDefaultAsync(x => x.Slug == input.CommunitySlug);
            var noHtml = Slug.HtmlToPlainText(input.Content); 
            var slug =   Slug.FriendlyUrlTitle(noHtml);
            var hasSlug = await _postRepository.GetAll()
                .FirstOrDefaultAsync(x => x.IsDeleted == false && x.Slug == slug);
            if (hasSlug != null)
            {
                var newContent = input.Content + "" + RandomString.GenerateString(3);
                slug = Slug.FriendlyUrlTitle(newContent);
            }
            var post = new Post
            {
               Content = input.Content,
               CommunityId = com.Id,
               UserId = input.UserId,
               ContentType = input.ContentType,
               Slug = slug
            };

            var user = await _userRepository.GetByIdAsync(input.UserId);

            if (input.ContentFile != null)
            {
                var path = await _blobService.InsertFile(input.ContentFile);
                post.MediaContentPath = path;
            }

            if (input.ContentType == ContentType.Link || input.ContentType == ContentType.YoutubeLink)
            {
                post.LinkUrl = input.LinkUrl;
            }
            await _postRepository.AddAsync(post);

            var mods = await _communityUserRepository.GetAll().Include(x=> x.User).Where(x =>
                    !x.IsDeleted && x.IsAdmin && x.CommunityId == com.Id && x.UserId != input.UserId )
                .ToListAsync();
            
            var msg = user.Username + " moderatörü olduğun" + " " + com.Name + " " + "topluluğunda paylaşım yaptı."+" "+ "https://saalla.com/t/"+ com.Slug ;
            var subject = "Bir yeni paylaşım";
            foreach (var mod in mods)
            {
                await _emailSender.SendEmail(mod.User.EmailAddress, subject, msg);
            }
            
            return post;
        }

        public async Task<PostDto> GetPostById(string slug,Guid? userId)
        {
            var post = await _postRepository.GetAll().Where(x => x.Slug == slug).Include(x => x.User)
                .Include(x=>x.Comments).ThenInclude(x=>x.Replies).ThenInclude(x=>x.ParentReply)
                .ThenInclude(x=>x.User)
                .Include(x=>x.Comments).ThenInclude(x=>x.Likes)
                .Select(x => new PostDto
                {
                    Id = x.Id,
                    Slug = x.Slug,
                    Content = x.Content,
                    LinkUrl = x.LinkUrl,
                    ContentType = x.ContentType,
                    UserPostVote = x.Votes.FirstOrDefault(u=>u.IsDeleted==false && u.UserId==userId && u.PostId==x.Id ),
                    ContentPath = x.MediaContentPath == null ? null : BlobService.BlobService.GetImageUrl(x.MediaContentPath),
                    CreatedDateTime = x.CreatedDate,
                    Community = new PostCommunityDto
                    {
                        Slug = x.Community.Slug,
                        Name = x.Community.Name,
                        LogoPath = BlobService.BlobService.GetImageUrl(x.Community.LogoPath)
                    },
                    UserInfo = new PostUserDto
                    {
                        UserName = x.User.Username,
                        ProfileImagePath = BlobService.BlobService.GetImageUrl(x.User.ProfileImagePath)
                    },
                    Comments = x.Comments.Where(c=>c.IsDeleted==false).Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        IsLoggedComment = c.UserId == userId,
                        IsLoggedLiked = c.Likes.Any(w=>w.IsDeleted==false && w.UserId==userId),
                        CreatedDateTime = c.CreatedDate,
                        LikeCount = c.Likes.Count(l=>l.IsDeleted==false),
                        CommentUserInfo = new CommentUserDto
                        {
                            Id = c.User.Id,
                            UserName = c.User.Username,
                            ProfileImagePath = BlobService.BlobService.GetImageUrl(c.User.ProfileImagePath)
                        },
                        Replies = c.Replies.Where(r=>r.IsDeleted==false).Select(r => new ReplyDto
                        {
                            Id = r.Id,
                            Content = r.Content,
                            Parent = new ParentDto
                            {
                                ParentReplyUserName = r.ParentId != null ? r.ParentReply.User.Username : null,
                                UserId = r.ParentId != null ? r.ParentReply.User.Id : (Guid?) null
                            }  ,
                            IsLoggedReply = r.UserId==userId,
                            IsLoggedLiked = r.Likes.Any(q=>q.IsDeleted==false && q.UserId==userId),
                            CreatedDateTime = r.CreatedDate,
                            LikeCount = r.Likes.Count(l=>l.IsDeleted==false),
                            ReplyUserInfo = new ReplyUserDto
                            {
                                Id = r.User.Id,
                                ProfileImagePath = BlobService.BlobService.GetImageUrl(r.User.ProfileImagePath),
                                UserName = r.User.Username
                            }
                        }).ToList()
                    }).ToList()
                }).FirstOrDefaultAsync();

            var dislikes = await _postVoteRepository.GetAll().Where(x => x.IsDeleted == false && x.PostId==post.Id && x.Value == -1).ToListAsync();
            var likes = await _postVoteRepository.GetAll().Where(x => x.IsDeleted == false && x.PostId==post.Id && x.Value == 1).ToListAsync();
            var voteCount = likes.Count - dislikes.Count;
            post.VoteCount = voteCount;
            return post;
        }

        public async Task Delete(Guid id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            post.IsDeleted = true;
            await _postRepository.UpdateAsync(post);
        }

        public async Task DeleteModerator(ModeratorDeleteDto input)
        {
            var isModerator = await _communityUserRepository.GetAll()
                .FirstOrDefaultAsync(x =>
                    x.IsDeleted == false && x.IsAdmin && x.UserId == input.ModeratorId &&
                    x.Community.Slug == input.Slug);
            
            if (isModerator == null)
            {
                throw new Exception("Bu kullanicinin yetkisi yok");
            };

            var post = await _postRepository.GetAll()
                .FirstOrDefaultAsync(x => x.IsDeleted == false && x.Id == input.PostId);
            if(post==null) throw new Exception("Post bulunamadi");
            
            
            post.IsDeleted = true;
            await _postRepository.UpdateAsync(post);

            var com = await _communityRepository.GetAll().FirstOrDefaultAsync(x => x.Slug == input.Slug);

            var model = new ModeratorOperation
            {
                Operation = "POST_DELETED",
                ModeratorId = input.ModeratorId,
                CommunityId = com.Id,
                PostId = input.PostId
            };
            await _moderatorOperationRepository.AddAsync(model);

        }

        public async Task<List<UserPostsDto>> GetUserPosts(IdOrUsernameDto input)
        {
            var result = await _postRepository.GetAll().
                Where(x => x.IsDeleted == false && x.User.Username == input.Username).
                Include(x=>x.Community).Select(
                x => new UserPostsDto
                {
                    Id = x.Id,
                    Slug = x.Slug,
                    Content = x.Content,
                    LinkUrl = x.LinkUrl,
                    UserPostVote = x.Votes.FirstOrDefault(p=>p.IsDeleted==false && p.UserId == input.Id  && p.PostId==x.Id ),
                    MediaContentPath = x.MediaContentPath == null ? null : BlobService.BlobService.GetImageUrl(x.MediaContentPath),
                    ContentType = x.ContentType,
                    CreatedDateTime = x.CreatedDate,
                    VoteCount = x.Votes.Count(v=>v.IsDeleted==false && v.Value==1) - x.Votes.Count(v=>v.IsDeleted==false && v.Value==-1),
                    Comments = x.Comments.Where(f=>f.IsDeleted==false).Select(f=> new Comment
                    {
                        ReplyCount = f.Replies.Count(v=>v.IsDeleted==false)
                    }).ToList(),
                    Community = new PostCommunityDto
                    {
                        Slug = x.Community.Slug,
                        Name = x.Community.Name,
                        LogoPath = BlobService.BlobService.GetImageUrl(x.Community.LogoPath)
                    }
                }).OrderByDescending(x=>x.Id).ToListAsync();
            return result;
        }

        public async Task<bool> HandlePin(string slug, bool value)
        {
            var post = await _postRepository.GetAll().Where(x => x.Slug == slug && !x.IsDeleted).FirstOrDefaultAsync();
            if(post==null) throw new Exception("Post bulunamadı");
            post.IsPinned = value;
            await _postRepository.UpdateAsync(post);
            return true;
        }

        public async Task<PostVote> Vote(CreateVoteDto input)
        {
            var post = await _postRepository.GetAll().Include(x=>x.User)
                .Where(x => x.IsDeleted == false && x.Id == input.PostId)
                .Include(x => x.Votes).FirstOrDefaultAsync();

            var postOwner = await _userRepository.GetByIdAsync(post.User.Id);
            
            var isExist = await _postVoteRepository.GetAll()
                .FirstOrDefaultAsync(x =>
                    x.IsDeleted == false && x.UserId == input.UserId && x.PostId == input.PostId);
            
            if (isExist != null)
            {
                if (input.Value == 0)
                {
                    await _postVoteRepository.DeleteAsync(isExist);
                    if (isExist.Value == 1)
                    {
                        postOwner.SPoint -= 1;
                    }
                    else
                    {
                        postOwner.SPoint += 1;
                    }

                    await _userRepository.UpdateAsync(postOwner);
                    return isExist;
                }
                isExist.Value = input.Value;
                await _postVoteRepository.UpdateAsync(isExist);
                if (input.Value == 1)
                {
                    postOwner.SPoint += 1;
                }
                else
                {
                    postOwner.SPoint -= 1;
                }

                await _userRepository.UpdateAsync(postOwner);
                return isExist;
            }
            
            var model = new PostVote
            {
                PostId = input.PostId,
                UserId = input.UserId,
                Value = input.Value
            };
            
            await _postVoteRepository.AddAsync(model);
            
            if (input.Value == 1)
            {
                postOwner.SPoint += 1;
            }
            else
            {
                postOwner.SPoint -= 1;
            }

            await _userRepository.UpdateAsync(postOwner);
            
            // notificaiton
            var user = await _userRepository.GetByIdAsync(input.UserId);
            
            var community = await _communityRepository.GetByIdAsync(post.CommunityId);
            if (post.UserId != user.Id)
            {
                var notify = new Notification
                {
                    TargetId = input.PostId,
                    OwnerUserId = post.UserId,
                    TargetName = community.Slug+"/"+post.Slug,
                    Type = NotifyContentType.PostVote,
                    Content = user.Username + " " + "sallamanı oyladı",
                    ImgPath = user.ProfileImagePath
                };
                await _notificationRepository.AddAsync(notify);
            }

            if (post.UserId != user.Id)
            {
                //email send
                //var voteCount = post.Votes.Count(x => x.IsDeleted == false);
                var subject = "Sallaman oylanıyor.";
                var url = "https://saalla.com/" + community.Slug + "/" + post.Slug;
                await _emailSender.SendEmail(post.User.EmailAddress, subject, user.Username + " kişisi Sallamana oy verdi :"+url);
            }
            return model;
        }

        public async Task<List<PostSlugs>> GetAllPostsSlug()
        {
            var slugs = await _postRepository.GetAll().Where(x => x.IsDeleted == false).Include(x => x.Community)
                .Select(x => new PostSlugs
                {
                    CSlug = x.Community.Slug,
                    PSlug = x.Slug
                }).ToListAsync();
            return slugs;
        }

        public async Task<List<GetAllPostDto>> HomePosts(Guid userId)
        {
            var result = await  _communityUserRepository.GetAll().Where(x => x.UserId == userId && x.IsDeleted==false )
                .Include(x => x.Community).ThenInclude(x => x.Posts)
                .Select(x => new Example
                {
                    Posts = x.Community.Posts.Where(p=>p.IsDeleted==false).Select(p => new GetAllPostDto
                    {
                        Id = p.Id,
                        Slug = p.Slug,
                        Content = p.Content,
                        ContentType = p.ContentType,
                        LinkUrl = p.LinkUrl,
                        VoteCount = p.Votes.Count(v=>v.IsDeleted==false && v.Value==1) - p.Votes.Count(v=>v.IsDeleted==false && v.Value==-1),
                        UserPostVote = p.Votes.FirstOrDefault(l=>l.UserId==userId && l.IsDeleted==false && l.PostId==p.Id ),
                        MediaContentPath = BlobService.BlobService.GetImageUrl(p.MediaContentPath),
                        CreatedDateTime = p.CreatedDate,
                        Community = new PostCommunityDto
                        {
                            Slug = x.Community.Slug,
                            Name = x.Community.Name,
                            LogoPath = BlobService.BlobService.GetImageUrl(x.Community.LogoPath)
                        },
                        User = new PostUserDto
                        {    
                            ProfileImagePath = BlobService.BlobService.GetImageUrl(p.User.ProfileImagePath),
                            UserName = p.User.Username    
                        },
                        Comments = p.Comments.Where(f=>f.IsDeleted==false).Select(f=> new Comment
                        {
                            ReplyCount = f.Replies.Count(v=>v.IsDeleted==false)
                        }).ToList()
                    }).OrderByDescending(p=>p.Id).ToList()
                }).ToListAsync();

            if (result.Count == 0) return await UnauthorizedHomePosts();
            
            var posts = new List<GetAllPostDto>();
            foreach (var item in result)
            {
                foreach (var post in item.Posts)
                {
                    posts.Add(post);
                }    
            }
            return posts;

        }

          public async Task<PagedResultDto<GetAllPostDto>> PagedHomePosts(PaginationParams input)
        {
            var result = await  _communityUserRepository.GetAll().Where(x => x.UserId == input.EntityId && x.IsDeleted==false )
                .Include(x => x.Community).ThenInclude(x => x.Posts)
                .Select(x => new Example
                {
                    Posts = x.Community.Posts.Where(p=>p.IsDeleted==false).Select(p => new GetAllPostDto
                    {
                        Id = p.Id,
                        Slug = p.Slug,
                        Content = p.Content,
                        ContentType = p.ContentType,
                        LinkUrl = p.LinkUrl,
                        PageNumber = input.PageNumber,
                        VoteCount = p.Votes.Count(v=>v.IsDeleted==false && v.Value==1) - p.Votes.Count(v=>v.IsDeleted==false && v.Value==-1),
                        UserPostVote = p.Votes.FirstOrDefault(l=>l.UserId== input.EntityId && l.IsDeleted==false && l.PostId==p.Id ),
                        MediaContentPath = p.MediaContentPath == null ? null : BlobService.BlobService.GetImageUrl(p.MediaContentPath),
                        CreatedDateTime = p.CreatedDate,
                        Community = new PostCommunityDto
                        {
                            Slug = x.Community.Slug,
                            Name = x.Community.Name,
                            LogoPath = x.Community.LogoPath == null ? null : BlobService.BlobService.GetImageUrl(x.Community.LogoPath)
                        },
                        User = new PostUserDto
                        {    
                            ProfileImagePath = BlobService.BlobService.GetImageUrl(p.User.ProfileImagePath),
                            UserName = p.User.Username    
                        },
                        Comments = p.Comments.Where(f=>f.IsDeleted==false).Select(f=> new Comment
                        {
                            ReplyCount = f.Replies.Count(v=>v.IsDeleted==false)
                        }).ToList()
                    }).ToList()
                }).ToListAsync();

            if (result.Count == 0) return await PagedUnauthorizedHomePosts(input);
            
            var posts = new List<GetAllPostDto>();
            foreach (var item in result)
            {
                foreach (var post in item.Posts)
                {
                    posts.Add(post);
                }
            }

            var aa = posts.OrderByDescending(x=>x.CreatedDateTime).Skip((input.PageNumber - 1) * input.PageSize).Take(input.PageSize).ToList();
            var hasNext = posts.Skip((input.PageNumber) * input.PageSize).Any();
            var bb = new PagedResultDto<GetAllPostDto> {Results = aa , HasNext = hasNext};
            return bb;

        }
          
        

        public async Task<List<GetAllPostDto>> UnauthorizedHomePosts()
        {
            var result = await _postRepository.GetAll().Where(x => x.IsDeleted == false)
                .Include(x=>x.User)
                .Include(x=>x.Community).Select(
                    x => new GetAllPostDto
                    {
                        Id = x.Id,
                        Content = x.Content,
                        LinkUrl = x.LinkUrl,
                        VoteCount = x.Votes.Count(v=>v.IsDeleted==false && v.Value==1) - x.Votes.Count(v=>v.IsDeleted==false && v.Value==-1),
                        MediaContentPath = BlobService.BlobService.GetImageUrl(x.MediaContentPath),
                        ContentType = x.ContentType,
                        CreatedDateTime = x.CreatedDate,
                        Comments = x.Comments.Where(f=>f.IsDeleted==false).Select(f=> new Comment
                        {
                            ReplyCount = f.Replies.Count(v=>v.IsDeleted==false)
                        }).ToList(),
                        Community = new PostCommunityDto
                        {
                            Slug = x.Community.Slug,
                            Name = x.Community.Name,
                            LogoPath = BlobService.BlobService.GetImageUrl(x.Community.LogoPath)
                        },
                        User = new PostUserDto
                        {
                            UserName = x.User.Username,
                            ProfileImagePath = BlobService.BlobService.GetImageUrl(x.User.ProfileImagePath)
                        }
                    }).ToListAsync();
            return result;
        }
        
        public async Task<PagedResultDto<GetAllPostDto>> PagedUnauthorizedHomePosts(PaginationParams input)
        {
            var result = await _postRepository.GetAll().Where(x => x.IsDeleted == false)
                .Include(x=>x.User)
                .Include(x=>x.Community).Select(
                    x => new GetAllPostDto
                    {
                        Id = x.Id,
                        Slug = x.Slug,
                        Content = x.Content,
                        LinkUrl = x.LinkUrl,
                        UserPostVote = x.Votes.FirstOrDefault(l=>l.UserId== input.EntityId && l.IsDeleted==false && l.PostId==x.Id ),
                        VoteCount = x.Votes.Count(v=>v.IsDeleted==false && v.Value==1) - x.Votes.Count(v=>v.IsDeleted==false && v.Value==-1),
                        MediaContentPath = x.MediaContentPath == null ? null : BlobService.BlobService.GetImageUrl(x.MediaContentPath),
                        ContentType = x.ContentType,
                        CreatedDateTime = x.CreatedDate,
                        PageNumber = input.PageNumber,
                        Comments = x.Comments.Where(f=>f.IsDeleted==false).Select(f=> new Comment
                        {
                            ReplyCount = f.Replies.Count(v=>v.IsDeleted==false)
                        }).ToList(),
                        Community = new PostCommunityDto
                        {
                           Slug = x.Community.Slug,
                            Name = x.Community.Name,
                            LogoPath = x.Community.LogoPath == null ? null : BlobService.BlobService.GetImageUrl(x.Community.LogoPath)
                        },
                        User = new PostUserDto
                        {
                            UserName = x.User.Username,
                            ProfileImagePath = BlobService.BlobService.GetImageUrl(x.User.ProfileImagePath)
                        }
                    }).OrderByDescending(x=>x.CreatedDateTime).Skip((input.PageNumber - 1) * input.PageSize).Take(input.PageSize).ToListAsync();
            var hasNext = await _postRepository.GetAll().Where(x => x.IsDeleted == false).Skip((input.PageNumber) * input.PageSize).AnyAsync();
            var bb = new PagedResultDto<GetAllPostDto> {Results = result , HasNext = hasNext};
            return bb;
        }
        
        public async Task<PagedResultDto<GetAllPostDto>> DailyTrends(PaginationParams input)
        {
            var result = await _postRepository.GetAll().Where(x => x.IsDeleted == false)
                .Include(x=>x.User)
                .Include(x=>x.Community).Select(
                    x => new GetAllPostDto    
                    {
                        Id = x.Id,
                        Slug = x.Slug,
                        Content = x.Content,
                        LinkUrl = x.LinkUrl,
                        UserPostVote = x.Votes.FirstOrDefault(l=>l.UserId== input.EntityId && l.IsDeleted==false && l.PostId==x.Id ),
                        VoteCount = x.Votes.Count(v=>v.IsDeleted==false && v.Value==1) - x.Votes.Count(v=>v.IsDeleted==false && v.Value==-1),
                        MediaContentPath = x.MediaContentPath == null ? null : BlobService.BlobService.GetImageUrl(x.MediaContentPath),
                        ContentType = x.ContentType,
                        CreatedDateTime = x.CreatedDate,
                        PageNumber = input.PageNumber,
                        Comments = x.Comments.Where(f=>f.IsDeleted==false).Select(f=> new Comment
                        {
                            ReplyCount = f.Replies.Count(v=>v.IsDeleted==false)
                        }).ToList(),
                        Community = new PostCommunityDto
                        {
                           Slug = x.Community.Slug,
                            Name = x.Community.Name,
                            LogoPath = x.Community.LogoPath == null ? null : BlobService.BlobService.GetImageUrl(x.Community.LogoPath)
                        },
                        User = new PostUserDto
                        {
                            UserName = x.User.Username,
                            ProfileImagePath = BlobService.BlobService.GetImageUrl(x.User.ProfileImagePath)
                        }
                    }).OrderByDescending(x=> x.CreatedDateTime).ThenByDescending(x=>x.VoteCount).
                Skip((input.PageNumber - 1) * input.PageSize).Take(input.PageSize).ToListAsync();
            var hasNext = await _postRepository.GetAll().Where(x => x.IsDeleted == false).Skip((input.PageNumber) * input.PageSize).AnyAsync();
            var bb = new PagedResultDto<GetAllPostDto> {Results = result , HasNext = hasNext};
            return bb;
        }
    }
}