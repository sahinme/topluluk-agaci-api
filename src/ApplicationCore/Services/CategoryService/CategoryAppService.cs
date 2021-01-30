using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.Categories;
using Microsoft.Nnn.ApplicationCore.Entities.Communities;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.CategoryService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.CommunityService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.UserService;

namespace Microsoft.Nnn.ApplicationCore.Services.CategoryService
{
    public class CategoryAppService:ICategoryAppService
    {
        private readonly IAsyncRepository<Category> _categoryRepository;
        private readonly IAsyncRepository<Community> _communityRepository;

        public CategoryAppService(IAsyncRepository<Category> categoryRepository,IAsyncRepository<Community> communityRepository)
        {    
            _categoryRepository = categoryRepository;
            _communityRepository = communityRepository;
        }
        
        public async Task<Category> CreateCategory(CreateCategoryDto input)
        {
            var slug = input.DisplayName.GenerateSlug();
            
            var category = new Category
            {
                DisplayName = input.DisplayName,
                Slug = slug
            };
            await _categoryRepository.AddAsync(category);
            return category;
        }

        public async Task<List<CategoryDto>> GetAllCategories()
        {
            var result = await _categoryRepository.GetAll().Where(x => x.IsDeleted == false).Select(x => new CategoryDto
            {
                Slug = x.Slug,
                DisplayName = x.DisplayName
            }).ToListAsync();
            return result;
        }

        public async Task<List<GetAllCommunityDto>> GetCommunitiesByCategory(string name)
        {
            var category = await _categoryRepository.GetAll().FirstOrDefaultAsync(x => !x.IsDeleted && x.Slug == name);
            var communities = await _communityRepository.GetAll()
                .Where(x => x.IsDeleted == false && x.CategoryId == category.Id)
                .Include(x=>x.Category)
                .Include(x => x.Users).ThenInclude(x=>x.User).Select(x => new GetAllCommunityDto
                {
                    Slug = x.Slug,
                    Name = x.Name,
                    Description = x.Description,
                    Category = new CategoryDto
                    {
                        Slug = x.Slug,
                        DisplayName = x.Category.DisplayName
                    },
                    LogoPath = x.LogoPath == null ? null : BlobService.BlobService.GetImageUrl(x.LogoPath),
                    MemberCount = x.Users.Count(m => m.IsDeleted==false),
                    //IsUserJoined = x.Users.Any(u => u.IsDeleted == false && u.UserId == userId)
                }).ToListAsync();
            return communities;
        }
    }
}