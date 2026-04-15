using Finexa.Application.Modules.Categories.DTOs;

namespace Finexa.Application.Modules.Categories.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetCategoriesAsync();
        Task CreateCategoryAsync(CreateCategoryDto dto);
    }
}