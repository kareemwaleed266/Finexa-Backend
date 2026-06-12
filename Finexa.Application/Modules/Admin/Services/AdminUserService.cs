using System.Text.Json;
using Finexa.Application.Common.Helpers;
using Finexa.Application.Common.Models;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Admin.DTOs;
using Finexa.Application.Modules.Admin.Interfaces;
using Finexa.Domain.Entities.Financial;
using Finexa.Domain.Entities.Identity;
using Finexa.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Admin.Services
{
    public class AdminUserService : IAdminUserService
    {
        private const string AdminRoleName = "Admin";
        private const string UserRoleName = "User";

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAdminAuditLogService _auditLogService;

        public AdminUserService(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAdminAuditLogService auditLogService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _auditLogService = auditLogService;
        }

        public async Task<PagedResult<AdminUserListItemDto>> GetUsersAsync(AdminUserFilterDto filter)
        {
            NormalizeFilter(filter);

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();

                query = query.Where(u =>
                    (u.Email != null && u.Email.ToLower().Contains(search)) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(search)));
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            }

            if (filter.IsLocked.HasValue)
            {
                var now = DateTimeOffset.UtcNow;

                query = filter.IsLocked.Value
                    ? query.Where(u => u.LockoutEnd != null && u.LockoutEnd > now)
                    : query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= now);
            }

            if (filter.CreatedFrom.HasValue)
            {
                var fromUtc = DateTimeHelper.ConvertClientLocalToUtc(filter.CreatedFrom.Value.Date);
                query = query.Where(u => u.CreatedAt >= fromUtc);
            }

            if (filter.CreatedTo.HasValue)
            {
                var toUtc = DateTimeHelper.ConvertClientLocalToUtc(filter.CreatedTo.Value.Date.AddDays(1));
                query = query.Where(u => u.CreatedAt < toUtc);
            }

            var users = await query.ToListAsync();

            if (!string.IsNullOrWhiteSpace(filter.Role))
            {
                var role = filter.Role.Trim();

                var filteredByRole = new List<AppUser>();

                foreach (var user in users)
                {
                    if (await _userManager.IsInRoleAsync(user, role))
                        filteredByRole.Add(user);
                }

                users = filteredByRole;
            }

            users = IsDescending(filter)
                ? users.OrderByDescending(u => u.CreatedAt).ToList()
                : users.OrderBy(u => u.CreatedAt).ToList();

            var totalCount = users.Count;

            var pagedUsers = users
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var items = new List<AdminUserListItemDto>();

            foreach (var user in pagedUsers)
            {
                items.Add(await MapToListItemDtoAsync(user));
            }

            await AddAuditLogAsync(
                AdminAuditAction.UserViewed,
                AdminTargetType.User,
                null,
                "Users list",
                "Admin viewed users list",
                null);

            return new PagedResult<AdminUserListItemDto>
            {
                Items = items,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<AdminUserDetailsDto> GetUserDetailsAsync(Guid userId)
        {
            var user = await GetUserOrThrowAsync(userId);

            var dto = await MapToDetailsDtoAsync(user);

            await AddAuditLogAsync(
                AdminAuditAction.UserViewed,
                AdminTargetType.User,
                user.Id,
                GetUserDisplayName(user),
                $"Admin viewed user details: {GetUserDisplayName(user)}",
                null);

            return dto;
        }

        public async Task LockUserAsync(Guid userId, AdminUserActionDto dto)
        {
            ValidateReason(dto);

            var user = await GetUserOrThrowAsync(userId);

            EnsureNotSelf(user.Id, "You cannot lock your own account");

            var oldValues = SerializeUserState(user);

            await _userManager.SetLockoutEnabledAsync(user, true);

            var result = await _userManager.SetLockoutEndDateAsync(
                user,
                DateTimeOffset.UtcNow.AddYears(100));

            EnsureIdentitySucceeded(result, "Failed to lock user");

            await AddAuditLogAsync(
                AdminAuditAction.UserLocked,
                AdminTargetType.User,
                user.Id,
                GetUserDisplayName(user),
                $"Admin locked user: {GetUserDisplayName(user)}",
                dto.Reason,
                oldValues,
                SerializeUserState(user));
        }

        public async Task UnlockUserAsync(Guid userId, AdminUserActionDto dto)
        {
            ValidateReason(dto);

            var user = await GetUserOrThrowAsync(userId);

            var oldValues = SerializeUserState(user);

            var result = await _userManager.SetLockoutEndDateAsync(user, null);

            EnsureIdentitySucceeded(result, "Failed to unlock user");

            await AddAuditLogAsync(
                AdminAuditAction.UserUnlocked,
                AdminTargetType.User,
                user.Id,
                GetUserDisplayName(user),
                $"Admin unlocked user: {GetUserDisplayName(user)}",
                dto.Reason,
                oldValues,
                SerializeUserState(user));
        }

        public async Task ActivateUserAsync(Guid userId, AdminUserActionDto dto)
        {
            ValidateReason(dto);

            var user = await GetUserOrThrowAsync(userId);

            var oldValues = SerializeUserState(user);

            if (user.IsActive)
                throw new InvalidOperationException("User is already active");

            user.IsActive = true;
            user.DeactivatedAt = null;
            user.DeactivationReason = null;

            var result = await _userManager.UpdateAsync(user);

            EnsureIdentitySucceeded(result, "Failed to activate user");

            await AddAuditLogAsync(
                AdminAuditAction.UserActivated,
                AdminTargetType.User,
                user.Id,
                GetUserDisplayName(user),
                $"Admin activated user: {GetUserDisplayName(user)}",
                dto.Reason,
                oldValues,
                SerializeUserState(user));
        }

        public async Task DeactivateUserAsync(Guid userId, AdminUserActionDto dto)
        {
            ValidateReason(dto);

            var user = await GetUserOrThrowAsync(userId);

            EnsureNotSelf(user.Id, "You cannot deactivate your own account");

            var oldValues = SerializeUserState(user);

            if (!user.IsActive)
                throw new InvalidOperationException("User is already deactivated");

            user.IsActive = false;
            user.DeactivatedAt = DateTime.UtcNow;
            user.DeactivationReason = dto.Reason.Trim();

            var result = await _userManager.UpdateAsync(user);

            EnsureIdentitySucceeded(result, "Failed to deactivate user");

            await AddAuditLogAsync(
                AdminAuditAction.UserDeactivated,
                AdminTargetType.User,
                user.Id,
                GetUserDisplayName(user),
                $"Admin deactivated user: {GetUserDisplayName(user)}",
                dto.Reason,
                oldValues,
                SerializeUserState(user));
        }

        public async Task MakeAdminAsync(Guid userId, AdminUserActionDto dto)
        {
            ValidateReason(dto);

            var user = await GetUserOrThrowAsync(userId);

            if (!await _roleManager.RoleExistsAsync(AdminRoleName))
                throw new InvalidOperationException("Admin role does not exist");

            if (await _userManager.IsInRoleAsync(user, AdminRoleName))
                throw new InvalidOperationException("User is already an admin");

            var oldRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRoleAsync(user, AdminRoleName);

            EnsureIdentitySucceeded(result, "Failed to promote user to admin");

            var newRoles = await _userManager.GetRolesAsync(user);

            await AddAuditLogAsync(
                AdminAuditAction.UserPromotedToAdmin,
                AdminTargetType.User,
                user.Id,
                GetUserDisplayName(user),
                $"Admin promoted user to Admin: {GetUserDisplayName(user)}",
                dto.Reason,
                JsonSerializer.Serialize(oldRoles),
                JsonSerializer.Serialize(newRoles));
        }

        public async Task RemoveAdminAsync(Guid userId, AdminUserActionDto dto)
        {
            ValidateReason(dto);

            var user = await GetUserOrThrowAsync(userId);

            EnsureNotSelf(user.Id, "You cannot remove Admin role from yourself");

            if (!await _userManager.IsInRoleAsync(user, AdminRoleName))
                throw new InvalidOperationException("User is not an admin");

            var adminUsers = await _userManager.GetUsersInRoleAsync(AdminRoleName);

            if (adminUsers.Count <= 1)
                throw new InvalidOperationException("Cannot remove the last admin");

            var oldRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.RemoveFromRoleAsync(user, AdminRoleName);

            EnsureIdentitySucceeded(result, "Failed to remove admin role");

            if (!await _userManager.IsInRoleAsync(user, UserRoleName) &&
                await _roleManager.RoleExistsAsync(UserRoleName))
            {
                var addUserRoleResult = await _userManager.AddToRoleAsync(user, UserRoleName);
                EnsureIdentitySucceeded(addUserRoleResult, "Failed to add User role");
            }

            var newRoles = await _userManager.GetRolesAsync(user);

            await AddAuditLogAsync(
                AdminAuditAction.UserRemovedFromAdmin,
                AdminTargetType.User,
                user.Id,
                GetUserDisplayName(user),
                $"Admin removed Admin role from user: {GetUserDisplayName(user)}",
                dto.Reason,
                JsonSerializer.Serialize(oldRoles),
                JsonSerializer.Serialize(newRoles));
        }

        private async Task<AdminUserListItemDto> MapToListItemDtoAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var transactionsCount = await _unitOfWork.Repository<Transaction, Guid>()
                .Query()
                .CountAsync(t => t.AppUserId == user.Id);

            var goalsCount = await _unitOfWork.Repository<Goal, Guid>()
                .Query()
                .CountAsync(g => g.AppUserId == user.Id);

            var billsCount = await _unitOfWork.Repository<BillSeries, Guid>()
                .Query()
                .CountAsync(b => b.AppUserId == user.Id);

            return new AdminUserListItemDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                IsLocked = IsUserLocked(user),
                LockoutEnd = user.LockoutEnd?.UtcDateTime,
                CreatedAt = DateTimeHelper.EnsureUtcKind(user.CreatedAt),
                Roles = roles.ToList(),
                TransactionsCount = transactionsCount,
                GoalsCount = goalsCount,
                BillsCount = billsCount
            };
        }

        private async Task<AdminUserDetailsDto> MapToDetailsDtoAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var transactionsCount = await _unitOfWork.Repository<Transaction, Guid>()
                .Query()
                .CountAsync(t => t.AppUserId == user.Id);

            var goalsCount = await _unitOfWork.Repository<Goal, Guid>()
                .Query()
                .CountAsync(g => g.AppUserId == user.Id);

            var billSeriesCount = await _unitOfWork.Repository<BillSeries, Guid>()
                .Query()
                .CountAsync(b => b.AppUserId == user.Id);

            var billOccurrences = await _unitOfWork.Repository<BillOccurrence, Guid>()
                .Query()
                .Where(b => b.AppUserId == user.Id)
                .ToListAsync();

            var userBalance = await _unitOfWork.Repository<UserBalance, Guid>()
                .Query()
                .FirstOrDefaultAsync(b => b.AppUserId == user.Id);

            return new AdminUserDetailsDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                DeactivatedAt = DateTimeHelper.EnsureUtcKind(user.DeactivatedAt),
                DeactivationReason = user.DeactivationReason,
                IsLocked = IsUserLocked(user),
                LockoutEnd = user.LockoutEnd?.UtcDateTime,
                AccessFailedCount = user.AccessFailedCount,
                CreatedAt = DateTimeHelper.EnsureUtcKind(user.CreatedAt),
                Roles = roles.ToList(),
                TransactionsCount = transactionsCount,
                GoalsCount = goalsCount,
                BillSeriesCount = billSeriesCount,
                BillOccurrencesCount = billOccurrences.Count,
                PaidBillOccurrencesCount = billOccurrences.Count(x => x.Status == BillOccurrenceStatus.Paid),
                OverdueBillOccurrencesCount = billOccurrences.Count(x =>
                    x.Status == BillOccurrenceStatus.Scheduled &&
                    DateTimeHelper.EnsureUtcKind(x.DueDate) < DateTime.UtcNow),
                TotalIncome = userBalance?.TotalIncome ?? 0,
                TotalExpense = userBalance?.TotalExpense ?? 0,
                TotalBalance = userBalance?.TotalBalance ?? 0
            };
        }

        private async Task<AppUser> GetUserOrThrowAsync(Guid userId)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            return user;
        }

        private void ValidateReason(AdminUserActionDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Reason))
                throw new ArgumentException("Reason is required");
        }

        private void EnsureNotSelf(Guid targetUserId, string message)
        {
            if (_currentUserService.UserId == targetUserId)
                throw new InvalidOperationException(message);
        }

        private static void EnsureIdentitySucceeded(IdentityResult result, string message)
        {
            if (result.Succeeded)
                return;

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));

            throw new InvalidOperationException($"{message}: {errors}");
        }

        private static bool IsUserLocked(AppUser user)
        {
            return user.LockoutEnd.HasValue &&
                   user.LockoutEnd.Value > DateTimeOffset.UtcNow;
        }

        private static string GetUserDisplayName(AppUser user)
        {
            return !string.IsNullOrWhiteSpace(user.Email)
                ? user.Email
                : user.UserName ?? user.Id.ToString();
        }

        private static void NormalizeFilter(AdminUserFilterDto filter)
        {
            if (filter.PageNumber <= 0)
                filter.PageNumber = 1;

            if (filter.PageSize <= 0)
                filter.PageSize = 10;

            if (filter.PageSize > 100)
                filter.PageSize = 100;
        }

        private static bool IsDescending(AdminUserFilterDto filter)
        {
            return filter.SortDirection?.ToString().Equals("Descending", StringComparison.OrdinalIgnoreCase) == true ||
                   filter.SortDirection?.ToString().Equals("Desc", StringComparison.OrdinalIgnoreCase) == true;
        }

        private static string SerializeUserState(AppUser user)
        {
            return JsonSerializer.Serialize(new
            {
                user.Id,
                user.Email,
                user.UserName,
                user.IsActive,
                user.LockoutEnd,
                user.EmailConfirmed,
                user.DeactivatedAt,
                user.DeactivationReason
            });
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
    }
}