using Finexa.Application.Modules.Admin.DTOs;
using Finexa.Application.Modules.Admin.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers.Admin
{
    [Route("api/admin/categories")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly IAdminCategoryService _adminCategoryService;

        public AdminCategoriesController(IAdminCategoryService adminCategoryService)
        {
            _adminCategoryService = adminCategoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] AdminCategoryFilterDto filter)
        {
            var categories = await _adminCategoryService.GetCategoriesAsync(filter);

            return Ok(categories);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCategoryDetails(Guid id)
        {
            var category = await _adminCategoryService.GetCategoryDetailsAsync(id);

            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSystemCategory(
            [FromBody] AdminCreateSystemCategoryDto dto)
        {
            var categoryId = await _adminCategoryService.CreateSystemCategoryAsync(dto);

            return Ok(new
            {
                message = "System category created successfully",
                categoryId
            });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateSystemCategory(
            Guid id,
            [FromBody] AdminUpdateSystemCategoryDto dto)
        {
            await _adminCategoryService.UpdateSystemCategoryAsync(id, dto);

            return Ok(new
            {
                message = "System category updated successfully"
            });
        }

        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> DeactivateCategory(
            Guid id,
            [FromBody] AdminCategoryActionDto dto)
        {
            await _adminCategoryService.DeactivateCategoryAsync(id, dto);

            return Ok(new
            {
                message = "System category deactivated successfully"
            });
        }

        [HttpPost("{id:guid}/reactivate")]
        public async Task<IActionResult> ReactivateCategory(
            Guid id,
            [FromBody] AdminCategoryActionDto dto)
        {
            await _adminCategoryService.ReactivateCategoryAsync(id, dto);

            return Ok(new
            {
                message = "System category reactivated successfully"
            });
        }
    }
}