using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Nnn.ApplicationCore.Entities.Categories;
using Microsoft.Nnn.ApplicationCore.Services.CategoryService.Dto;
using Microsoft.Nnn.ApplicationCore.Services.CommunityService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.CategoryService
{
    public interface ICategoryAppService
    {
        Task<Category> CreateCategory(CreateCategoryDto input);
        Task<List<CategoryDto>> GetAllCategories();
        Task<List<GetAllCommunityDto>> GetCommunitiesByCategory(string name);
    }
}