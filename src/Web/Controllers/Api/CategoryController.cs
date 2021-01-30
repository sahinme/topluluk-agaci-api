using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Nnn.ApplicationCore.Services.CategoryService;
using Microsoft.Nnn.ApplicationCore.Services.CategoryService.Dto;
using Microsoft.Nnn.Web.Identity;

namespace Microsoft.Nnn.Web.Controllers.Api
{
    public class CategoryController:BaseApiController
    {
        private readonly ICategoryAppService _categoryAppService;

        public CategoryController(ICategoryAppService categoryAppService)
        {
            _categoryAppService = categoryAppService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto input)
        {
            var token = GetToken();
            var userType = LoginHelper.GetClaim(token, "UserRole");
            if (userType != "Admin") return Unauthorized();
            
            var result = await _categoryAppService.CreateCategory(input);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryAppService.GetAllCategories();
            
            return Ok(result);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetCommunities(string name)
        {
            var result = await _categoryAppService.GetCommunitiesByCategory(name);
            return Ok(result);
        }
    }
}