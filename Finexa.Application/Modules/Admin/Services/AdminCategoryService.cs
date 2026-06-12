using System.Text.Json;
using Finexa.Application.Common.Helpers;
using Finexa.Application.Common.Models;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Admin.DTOs;
using Finexa.Application.Modules.Admin.Interfaces;
using Finexa.Domain.Entities;
using Finexa.Domain.Entities.Financial;
using Finexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Admin.Services
{
    public class AdminCategoryService : IAdminCategoryService
    {
        private static readonly string[] ProtectedCategoryNames =
        {
            "Balance Adjustment",
            "Other Income",
            "Other Expense",
            "Goals",
            "Bills",
            "Receipt"
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAdminAuditLogService _auditLogService;

        public AdminCategoryService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAdminAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _auditLogService = auditLogService;
        }

        public async Task<PagedResult<AdminCategoryDto>> GetCategoriesAsync(AdminCategoryFilterDto filter)
        {
            NormalizeFilter(filter);

            var query = _unitOfWork.Repository<Category, Guid>()
                .Query()
                .Where(c => c.AppUserId == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();

                query = query.Where(c => c.Name.ToLower().Contains(search));
            }

            if (filter.Type.HasValue)
            {
                query = query.Where(c => c.Type == filter.Type.Value);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == filter.IsActive.Value);
            }

            if (filter.IsDefault.HasValue)
            {
                query = query.Where(c => c.IsDefault == filter.IsDefault.Value);
            }

            var totalCount = await query.CountAsync();

            query = IsDescending(filter)
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.Name);

            var categories = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var items = new List<AdminCategoryDto>();

            foreach (var category in categories)
            {
                items.Add(await MapToDtoAsync(category));
            }

            await AddAuditLogAsync(
                AdminAuditAction.SystemCategoryUpdated,
                AdminTargetType.Category,
                null,
                "System Categories",
                "Admin viewed system categories list",
                null);

            return new PagedResult<AdminCategoryDto>
            {
                Items = items,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<AdminCategoryDto> GetCategoryDetailsAsync(Guid categoryId)
        {
            var category = await GetSystemCategoryOrThrowAsync(categoryId, withTracking: false);

            var dto = await MapToDtoAsync(category);

            await AddAuditLogAsync(
                AdminAuditAction.SystemCategoryUpdated,
                AdminTargetType.Category,
                category.Id,
                category.Name,
                $"Admin viewed system category details: {category.Name}",
                null);

            return dto;
        }

        public async Task<Guid> CreateSystemCategoryAsync(AdminCreateSystemCategoryDto dto)
        {
            ValidateCreateDto(dto);

            var normalizedName = dto.Name.Trim().ToLower();

            var exists = await _unitOfWork.Repository<Category, Guid>()
                .ExistsAsync(c =>
                    c.AppUserId == null &&
                    c.Name.ToLower() == normalizedName);

            if (exists)
                throw new InvalidOperationException("System category with the same name already exists");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Type = dto.Type,
                IsDefault = true,
                IsActive = true,
                AppUserId = null,
                CreatedBy = GetAuditUser()
            };

            await _unitOfWork.Repository<Category, Guid>().AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            await AddAuditLogAsync(
                AdminAuditAction.SystemCategoryCreated,
                AdminTargetType.Category,
                category.Id,
                category.Name,
                $"Admin created system category: {category.Name}",
                null,
                null,
                JsonSerializer.Serialize(new
                {
                    category.Id,
                    category.Name,
                    category.Type,
                    category.IsDefault,
                    category.IsActive
                }));

            return category.Id;
        }

        public async Task UpdateSystemCategoryAsync(Guid categoryId, AdminUpdateSystemCategoryDto dto)
        {
            ValidateUpdateDto(dto);

            var category = await GetSystemCategoryOrThrowAsync(categoryId, withTracking: true);

            EnsureNotProtected(category, "Protected system category cannot be updated");

            var normalizedName = dto.Name.Trim().ToLower();

            var duplicateExists = await _unitOfWork.Repository<Category, Guid>()
                .ExistsAsync(c =>
                    c.AppUserId == null &&
                    c.Id != categoryId &&
                    c.Name.ToLower() == normalizedName);

            if (duplicateExists)
                throw new InvalidOperationException("System category with the same name already exists");

            var usage = await GetCategoryUsageAsync(category.Id);

            if (usage.IsUsed && category.Type != dto.Type)
                throw new InvalidOperationException("Cannot change category type because it is already used");

            var oldValuesJson = JsonSerializer.Serialize(new
            {
                category.Id,
                category.Name,
                category.Type,
                category.IsDefault,
                category.IsActive
            });

            category.Name = dto.Name.Trim();
            category.Type = dto.Type;
            category.LastModifiedBy = GetAuditUser();

            await _unitOfWork.SaveChangesAsync();

            await AddAuditLogAsync(
                AdminAuditAction.SystemCategoryUpdated,
                AdminTargetType.Category,
                category.Id,
                category.Name,
                $"Admin updated system category: {category.Name}",
                null,
                oldValuesJson,
                JsonSerializer.Serialize(new
                {
                    category.Id,
                    category.Name,
                    category.Type,
                    category.IsDefault,
                    category.IsActive
                }));
        }

        public async Task DeactivateCategoryAsync(Guid categoryId, AdminCategoryActionDto dto)
        {
            ValidateReason(dto);

            var category = await GetSystemCategoryOrThrowAsync(categoryId, withTracking: true);

            EnsureNotProtected(category, "Protected system category cannot be deactivated");

            if (!category.IsActive)
                throw new InvalidOperationException("Category is already inactive");

            var oldValuesJson = JsonSerializer.Serialize(new
            {
                category.Id,
                category.Name,
                category.Type,
                category.IsDefault,
                category.IsActive
            });

            category.IsActive = false;
            category.LastModifiedBy = GetAuditUser();

            await _unitOfWork.SaveChangesAsync();

            await AddAuditLogAsync(
                AdminAuditAction.SystemCategoryDeactivated,
                AdminTargetType.Category,
                category.Id,
                category.Name,
                $"Admin deactivated system category: {category.Name}",
                dto.Reason,
                oldValuesJson,
                JsonSerializer.Serialize(new
                {
                    category.Id,
                    category.Name,
                    category.Type,
                    category.IsDefault,
                    category.IsActive
                }));
        }

        public async Task ReactivateCategoryAsync(Guid categoryId, AdminCategoryActionDto dto)
        {
            ValidateReason(dto);

            var category = await GetSystemCategoryOrThrowAsync(categoryId, withTracking: true);

            if (category.IsActive)
                throw new InvalidOperationException("Category is already active");

            var oldValuesJson = JsonSerializer.Serialize(new
            {
                category.Id,
                category.Name,
                category.Type,
                category.IsDefault,
                category.IsActive
            });

            category.IsActive = true;
            category.LastModifiedBy = GetAuditUser();

            await _unitOfWork.SaveChangesAsync();

            await AddAuditLogAsync(
                AdminAuditAction.SystemCategoryReactivated,
                AdminTargetType.Category,
                category.Id,
                category.Name,
                $"Admin reactivated system category: {category.Name}",
                dto.Reason,
                oldValuesJson,
                JsonSerializer.Serialize(new
                {
                    category.Id,
                    category.Name,
                    category.Type,
                    category.IsDefault,
                    category.IsActive
                }));
        }

        private async Task<Category> GetSystemCategoryOrThrowAsync(Guid categoryId, bool withTracking)
        {
            var category = await _unitOfWork.Repository<Category, Guid>()
                .Query(withTracking)
                .FirstOrDefaultAsync(c =>
                    c.Id == categoryId &&
                    c.AppUserId == null);

            if (category == null)
                throw new KeyNotFoundException("System category not found");

            return category;
        }

        private async Task<AdminCategoryDto> MapToDtoAsync(Category category)
        {
            var usage = await GetCategoryUsageAsync(category.Id);

            return new AdminCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type,
                IsDefault = category.IsDefault,
                IsActive = category.IsActive,
                IsProtected = IsProtectedCategory(category.Name),
                TransactionsCount = usage.TransactionsCount,
                BillSeriesCount = usage.BillSeriesCount,
                IsUsed = usage.IsUsed,
                CreatedAt = DateTimeHelper.EnsureUtcKind(category.CreatedAt),
                LastModifiedAt = DateTimeHelper.EnsureUtcKind(category.LastModifiedAt)
            };
        }

        private async Task<CategoryUsage> GetCategoryUsageAsync(Guid categoryId)
        {
            var transactionsCount = await _unitOfWork.Repository<Transaction, Guid>()
                .Query()
                .CountAsync(t => t.CategoryId == categoryId);

            var billSeriesCount = await _unitOfWork.Repository<BillSeries, Guid>()
                .Query()
                .CountAsync(b => b.CategoryId == categoryId);

            return new CategoryUsage(
                transactionsCount,
                billSeriesCount);
        }

        private static void ValidateCreateDto(AdminCreateSystemCategoryDto dto)
        {
            if (dto == null)
                throw new ArgumentException("Category data is required");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Category name is required");
        }

        private static void ValidateUpdateDto(AdminUpdateSystemCategoryDto dto)
        {
            if (dto == null)
                throw new ArgumentException("Category data is required");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Category name is required");
        }

        private static void ValidateReason(AdminCategoryActionDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Reason))
                throw new ArgumentException("Reason is required");
        }

        private static void EnsureNotProtected(Category category, string message)
        {
            if (IsProtectedCategory(category.Name))
                throw new InvalidOperationException(message);
        }

        private static bool IsProtectedCategory(string categoryName)
        {
            return ProtectedCategoryNames.Any(x =>
                x.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
        }

        private static void NormalizeFilter(AdminCategoryFilterDto filter)
        {
            if (filter.PageNumber <= 0)
                filter.PageNumber = 1;

            if (filter.PageSize <= 0)
                filter.PageSize = 10;

            if (filter.PageSize > 100)
                filter.PageSize = 100;
        }

        private static bool IsDescending(AdminCategoryFilterDto filter)
        {
            return filter.SortDirection?.ToString().Equals("Descending", StringComparison.OrdinalIgnoreCase) == true ||
                   filter.SortDirection?.ToString().Equals("Desc", StringComparison.OrdinalIgnoreCase) == true;
        }

        private async Task AddAuditLogAsync(
            AdminAuditAction action,
            AdminTargetType targetType,
            Guid? targetId,
            string? targetDisplayName,
            string description,
            string? reason,
            string? oldValuesJson = null,
            string? newValuesJson = null)
        {
            await _auditLogService.AddAsync(new CreateAdminAuditLogDto
            {
                AdminUserId = _currentUserService.UserId == Guid.Empty
                    ? null
                    : _currentUserService.UserId,
                AdminEmail = _currentUserService.Email,
                Action = action,
                TargetType = targetType,
                TargetId = targetId,
                TargetDisplayName = targetDisplayName,
                Description = description,
                Reason = reason,
                OldValuesJson = oldValuesJson,
                NewValuesJson = newValuesJson
            });
        }

        private string GetAuditUser()
        {
            return string.IsNullOrWhiteSpace(_currentUserService.Email)
                ? "System"
                : _currentUserService.Email;
        }

        private sealed record CategoryUsage(
            int TransactionsCount,
            int BillSeriesCount)
        {
            public bool IsUsed => TransactionsCount > 0 || BillSeriesCount > 0;
        }
    }
}