using Finexa.Application.Common.Models;
using Finexa.Application.Modules.Admin.DTOs;

namespace Finexa.Application.Modules.Admin.Interfaces
{
    public interface IAdminCategoryService
    {
        Task<PagedResult<AdminCategoryDto>> GetCategoriesAsync(AdminCategoryFilterDto filter);

        Task<AdminCategoryDto> GetCategoryDetailsAsync(Guid categoryId);

        Task<Guid> CreateSystemCategoryAsync(AdminCreateSystemCategoryDto dto);

        Task UpdateSystemCategoryAsync(Guid categoryId, AdminUpdateSystemCategoryDto dto);

        Task DeactivateCategoryAsync(Guid categoryId, AdminCategoryActionDto dto);

        Task ReactivateCategoryAsync(Guid categoryId, AdminCategoryActionDto dto);
    }
}