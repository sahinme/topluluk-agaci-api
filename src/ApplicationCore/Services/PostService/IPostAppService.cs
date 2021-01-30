using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Nnn.ApplicationCore.Entities.Posts;
using Microsoft.Nnn.ApplicationCore.Entities.PostVotes;
using Microsoft.Nnn.ApplicationCore.Services.Dto;
using Microsoft.Nnn.ApplicationCore.Services.PostAppService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.PostService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.PostService
{
    public interface IPostAppService
    {
        Task<Post> CreatePost(CreatePostDto input);
        Task<PostDto> GetPostById(string slug,Guid? userId);
        Task Delete(Guid id);
        Task DeleteModerator(ModeratorDeleteDto input);
        Task<List<UserPostsDto>> GetUserPosts(IdOrUsernameDto input);
        Task<List<GetAllPostDto>> HomePosts(Guid userId);
        Task<PagedResultDto<GetAllPostDto>> PagedHomePosts(PaginationParams input);
        Task<PagedResultDto<GetAllPostDto>> PagedUnauthorizedHomePosts(PaginationParams input);
        Task<PagedResultDto<GetAllPostDto>> DailyTrends(PaginationParams input);
        Task<bool> HandlePin(string slug,bool value); // value must be true or false. If it is true post will be pinned.
        Task<PostVote> Vote(CreateVoteDto input);
        Task<List<PostSlugs>> GetAllPostsSlug();
    }
}