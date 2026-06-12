using Finexa.Application.Common.Helpers;
using Finexa.Application.Common.Models;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Admin.DTOs;
using Finexa.Application.Modules.Admin.Interfaces;
using Finexa.Domain.Entities.Admin;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Admin.Services
{
    public class AdminAuditLogService : IAdminAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public AdminAuditLogService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task AddAsync(CreateAdminAuditLogDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ArgumentException("Audit log description is required");

            var currentUserId = _currentUserService.UserId;

            var log = new AdminAuditLog
            {
                Id = Guid.NewGuid(),
                AdminUserId = dto.AdminUserId ?? (currentUserId == Guid.Empty ? null : currentUserId),
                AdminEmail = string.IsNullOrWhiteSpace(dto.AdminEmail)
                    ? _currentUserService.Email
                    : dto.AdminEmail.Trim(),
                Action = dto.Action,
                TargetType = dto.TargetType,
                TargetId = dto.TargetId,
                TargetDisplayName = string.IsNullOrWhiteSpace(dto.TargetDisplayName)
                    ? null
                    : dto.TargetDisplayName.Trim(),
                Description = dto.Description.Trim(),
                Reason = string.IsNullOrWhiteSpace(dto.Reason)
                    ? null
                    : dto.Reason.Trim(),
                OldValuesJson = dto.OldValuesJson,
                NewValuesJson = dto.NewValuesJson,
                IpAddress = string.IsNullOrWhiteSpace(dto.IpAddress)
                    ? null
                    : dto.IpAddress.Trim(),
                UserAgent = string.IsNullOrWhiteSpace(dto.UserAgent)
                    ? null
                    : dto.UserAgent.Trim(),
                CreatedBy = string.IsNullOrWhiteSpace(dto.AdminEmail)
                    ? GetAuditUser()
                    : dto.AdminEmail.Trim()
            };

            await _unitOfWork.Repository<AdminAuditLog, Guid>().AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PagedResult<AdminAuditLogDto>> GetLogsAsync(AdminAuditLogFilterDto filter)
        {
            if (filter.PageNumber <= 0)
                filter.PageNumber = 1;

            if (filter.PageSize <= 0)
                filter.PageSize = 10;

            if (filter.PageSize > 100)
                filter.PageSize = 100;

            var query = _unitOfWork.Repository<AdminAuditLog, Guid>()
                .Query()
                .AsQueryable();

            if (filter.AdminUserId.HasValue)
            {
                query = query.Where(x => x.AdminUserId == filter.AdminUserId.Value);
            }

            if (filter.Action.HasValue)
            {
                query = query.Where(x => x.Action == filter.Action.Value);
            }

            if (filter.TargetType.HasValue)
            {
                query = query.Where(x => x.TargetType == filter.TargetType.Value);
            }

            if (filter.TargetId.HasValue)
            {
                query = query.Where(x => x.TargetId == filter.TargetId.Value);
            }

            if (filter.FromDate.HasValue)
            {
                var fromUtc = DateTimeHelper.ConvertClientLocalToUtc(filter.FromDate.Value.Date);
                query = query.Where(x => x.CreatedAt >= fromUtc);
            }

            if (filter.ToDate.HasValue)
            {
                var toUtc = DateTimeHelper.ConvertClientLocalToUtc(filter.ToDate.Value.Date.AddDays(1));
                query = query.Where(x => x.CreatedAt < toUtc);
            }

            var totalCount = await query.CountAsync();

            var isDescending =
                filter.SortDirection?.ToString().Equals("Descending", StringComparison.OrdinalIgnoreCase) == true ||
                filter.SortDirection?.ToString().Equals("Desc", StringComparison.OrdinalIgnoreCase) == true;

            query = isDescending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt);

            var logs = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<AdminAuditLogDto>
            {
                Items = logs.Select(MapToDto).ToList(),
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        private static AdminAuditLogDto MapToDto(AdminAuditLog log)
        {
            return new AdminAuditLogDto
            {
                Id = log.Id,
                AdminUserId = log.AdminUserId,
                AdminEmail = log.AdminEmail,
                Action = log.Action,
                TargetType = log.TargetType,
                TargetId = log.TargetId,
                TargetDisplayName = log.TargetDisplayName,
                Description = log.Description,
                Reason = log.Reason,
                OldValuesJson = log.OldValuesJson,
                NewValuesJson = log.NewValuesJson,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                CreatedAt = DateTimeHelper.EnsureUtcKind(log.CreatedAt)
            };
        }

        private string GetAuditUser()
        {
            return string.IsNullOrWhiteSpace(_currentUserService.Email)
                ? "System"
                : _currentUserService.Email;
        }
    }
}