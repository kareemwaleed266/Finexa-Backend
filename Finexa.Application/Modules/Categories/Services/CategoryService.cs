using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Categories.DTOs;
using Finexa.Application.Modules.Categories.Interfaces;
using Finexa.Domain.Entities.Financial;
using Finexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Categories.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CategoryService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            var categories = await _unitOfWork.Repository<Category, Guid>()
                .Query()
                .Where(c =>
                    c.IsActive &&
                    (c.AppUserId == null || c.AppUserId == userId))
                .OrderBy(c => c.Type)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                CategoryType = c.Type,
                IsBillCategory = c.IsBillCategory
            }).ToList();
        }

        public async Task CreateCategoryAsync(CreateCategoryDto dto)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            if (dto == null)
                throw new ArgumentException("Category data is required");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Category name is required");

            var categoryName = dto.Name.Trim();

            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var exists = await categoryRepo
                .Query()
                .AnyAsync(c =>
                    c.Type == dto.CategoryType &&
                    (c.AppUserId == null || c.AppUserId == userId) &&
                    c.Name.ToLower() == categoryName.ToLower());

            if (exists)
                throw new InvalidOperationException("Category already exists");

            if (dto.IsBillCategory && dto.CategoryType != TransactionType.Expense)
                throw new InvalidOperationException("Bill category must be an expense category");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = categoryName,
                AppUserId = userId,
                Type = dto.CategoryType,
                IsDefault = false,
                IsActive = true,
                IsBillCategory = dto.CategoryType == TransactionType.Expense && dto.IsBillCategory,
                CreatedBy = string.IsNullOrWhiteSpace(_currentUser.CurrentUserDisplayName)
                    ? _currentUser.Email
                    : _currentUser.CurrentUserDisplayName
            };

            await categoryRepo.AddAsync(category);

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<List<CategoryDto>> GetBillCategoriesAsync()
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            var categories = await _unitOfWork.Repository<Category, Guid>()
                .Query()
                .Where(c =>
                    c.IsActive &&
                    c.IsBillCategory &&
                    c.Type == TransactionType.Expense &&
                    (c.AppUserId == null || c.AppUserId == userId))
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                CategoryType = c.Type,
                IsBillCategory = c.IsBillCategory
            }).ToList();
        }
    }
}