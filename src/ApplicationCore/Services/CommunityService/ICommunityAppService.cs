using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Nnn.ApplicationCore.Entities.Communities;
using Microsoft.Nnn.ApplicationCore.Services.CommunityService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.CommunityService
{
    public interface ICommunityAppService
    {
        Task<Response> CreateCommunity(CreateCommunity input);
        Task<List<GetAllCommunityDto>> GetAll();
        Task<Community> Update(UpdateCommunity input);
        Task<CommunityDto> GetById(string slug,Guid? userId);
        Task<PagedResultDto<CommunityPostDto>> GetPosts(PageDtoCommunity input);
        Task<List<CommunityPostDto>> GetPinnedPosts(PageDtoCommunity input);
        Task<List<GetAllCommunityDto>> GetPopulars(Guid? userId);
        Task<List<GetAllCommunityDto>> OfModerators(Guid userId);
        Task<List<CommunityUserDto>> Users(string slug);
        Task<List<SearchDto>> Search(string text);
        Task<Response> AddModerator(AddModeratorDto input);
    }
}