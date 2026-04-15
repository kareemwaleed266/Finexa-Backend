using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Categories.DTOs;
using Finexa.Application.Modules.Categories.Interfaces;

namespace Finexa.Application.Modules.Categories.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CategoryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var userId = _currentUser.UserId;

            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var categories = await categoryRepo
                .WhereAsync(c => c.AppUserId == null || c.AppUserId == userId);

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
        }

        public async Task CreateCategoryAsync(CreateCategoryDto dto)
        {
            var userId = _currentUser.UserId;
            var createdBy = _currentUser.CurrentUserDisplayName;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException();

            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var exists = await categoryRepo.FirstOrDefaultAsync(c =>
                c.Name == dto.Name &&
                (c.AppUserId == null || c.AppUserId == userId));

            if (exists != null)
                throw new Exception("Category already exists");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                AppUserId = userId,
                CreatedBy = createdBy
            };

            await categoryRepo.AddAsync(category);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}