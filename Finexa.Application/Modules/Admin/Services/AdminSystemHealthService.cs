using Finexa.Application.Common.Helpers;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Admin.DTOs;
using Finexa.Application.Modules.Admin.Interfaces;
using Finexa.Domain.Entities.Admin;
using Finexa.Domain.Entities.Financial;
using Finexa.Domain.Entities.Identity;
using Finexa.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Admin.Services
{
    public class AdminSystemHealthService : IAdminSystemHealthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAdminAuditLogService _auditLogService;

        public AdminSystemHealthService(
            UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAdminAuditLogService auditLogService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _auditLogService = auditLogService;
        }

        public async Task<AdminSystemHealthDto> GetHealthAsync()
        {
            var checkedAt = DateTime.UtcNow;

            var latestJob = await _unitOfWork.Repository<SystemJobLog, Guid>()
                .Query()
                .OrderByDescending(j => j.StartedAt)
                .FirstOrDefaultAsync();

            var last24Hours = checkedAt.AddHours(-24);

            var failedJobsLast24Hours = await _unitOfWork.Repository<SystemJobLog, Guid>()
                .Query()
                .CountAsync(j =>
                    j.Status == SystemJobStatus.Failed &&
                    j.StartedAt >= last24Hours);

            var health = new AdminSystemHealthDto
            {
                DatabaseAvailable = true,
                CheckedAt = checkedAt,
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalTransactions = await _unitOfWork.Repository<Transaction, Guid>().Query().CountAsync(),
                TotalBillSeries = await _unitOfWork.Repository<BillSeries, Guid>().Query().CountAsync(),
                TotalBillOccurrences = await _unitOfWork.Repository<BillOccurrence, Guid>().Query().CountAsync(),
                FailedJobsLast24Hours = failedJobsLast24Hours,
                LatestJobStatus = latestJob?.Status,
                LatestJobStartedAt = latestJob == null
                    ? null
                    : DateTimeHelper.EnsureUtcKind(latestJob.StartedAt),
                LatestJobFinishedAt = latestJob == null
                    ? null
                    : DateTimeHelper.EnsureUtcKind(latestJob.FinishedAt),
                Status = GetSystemStatus(failedJobsLast24Hours, latestJob)
            };

            await _auditLogService.AddAsync(new CreateAdminAuditLogDto
            {
                AdminUserId = _currentUserService.UserId == Guid.Empty
                    ? null
                    : _currentUserService.UserId,
                AdminEmail = _currentUserService.Email,
                Action = AdminAuditAction.DashboardViewed,
                TargetType = AdminTargetType.System,
                Description = "Admin viewed system health"
            });

            return health;
        }

        private static string GetSystemStatus(
            int failedJobsLast24Hours,
            SystemJobLog? latestJob)
        {
            if (failedJobsLast24Hours > 0)
                return "Warning";

            if (latestJob != null && latestJob.Status == SystemJobStatus.Failed)
                return "Warning";

            return "Healthy";
        }
    }
}