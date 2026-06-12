using Finexa.Application.Common.Helpers;
using Finexa.Application.Common.Models;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Admin.DTOs;
using Finexa.Application.Modules.Admin.Interfaces;
using Finexa.Domain.Entities.Admin;
using Finexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Admin.Services
{
    public class AdminJobLogService : IAdminJobLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminJobLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> StartJobAsync(
            SystemJobName jobName,
            string triggeredBy = "System",
            string? metadataJson = null)
        {
            var now = DateTime.UtcNow;

            var log = new SystemJobLog
            {
                Id = Guid.NewGuid(),
                JobName = jobName,
                Status = SystemJobStatus.Running,
                StartedAt = now,
                TriggeredBy = string.IsNullOrWhiteSpace(triggeredBy)
                    ? "System"
                    : triggeredBy.Trim(),
                MetadataJson = metadataJson,
                CreatedBy = string.IsNullOrWhiteSpace(triggeredBy)
                    ? "System"
                    : triggeredBy.Trim()
            };

            await _unitOfWork.Repository<SystemJobLog, Guid>().AddAsync(log);
            await _unitOfWork.SaveChangesAsync();

            return log.Id;
        }

        public async Task MarkJobSucceededAsync(
            Guid jobLogId,
            int processedCount = 0,
            int createdCount = 0,
            int updatedCount = 0,
            string? metadataJson = null)
        {
            var log = await GetTrackedJobLogAsync(jobLogId);

            var finishedAt = DateTime.UtcNow;

            log.Status = SystemJobStatus.Succeeded;
            log.FinishedAt = finishedAt;
            log.DurationMs = (long)(finishedAt - log.StartedAt).TotalMilliseconds;
            log.ProcessedCount = processedCount;
            log.CreatedCount = createdCount;
            log.UpdatedCount = updatedCount;
            log.FailedCount = 0;
            log.ErrorMessage = null;

            if (!string.IsNullOrWhiteSpace(metadataJson))
                log.MetadataJson = metadataJson;

            log.LastModifiedBy = "System";

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkJobFailedAsync(
            Guid jobLogId,
            string errorMessage,
            int processedCount = 0,
            int createdCount = 0,
            int updatedCount = 0,
            int failedCount = 1,
            string? metadataJson = null)
        {
            var log = await GetTrackedJobLogAsync(jobLogId);

            var finishedAt = DateTime.UtcNow;

            log.Status = SystemJobStatus.Failed;
            log.FinishedAt = finishedAt;
            log.DurationMs = (long)(finishedAt - log.StartedAt).TotalMilliseconds;
            log.ProcessedCount = processedCount;
            log.CreatedCount = createdCount;
            log.UpdatedCount = updatedCount;
            log.FailedCount = failedCount;
            log.ErrorMessage = string.IsNullOrWhiteSpace(errorMessage)
                ? "Unknown error"
                : errorMessage;

            if (!string.IsNullOrWhiteSpace(metadataJson))
                log.MetadataJson = metadataJson;

            log.LastModifiedBy = "System";

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PagedResult<SystemJobLogDto>> GetLogsAsync(SystemJobLogFilterDto filter)
        {
            if (filter.PageNumber <= 0)
                filter.PageNumber = 1;

            if (filter.PageSize <= 0)
                filter.PageSize = 10;

            if (filter.PageSize > 100)
                filter.PageSize = 100;

            var query = _unitOfWork.Repository<SystemJobLog, Guid>()
                .Query()
                .AsQueryable();

            if (filter.JobName.HasValue)
            {
                query = query.Where(x => x.JobName == filter.JobName.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (filter.FromDate.HasValue)
            {
                var fromUtc = DateTimeHelper.ConvertClientLocalToUtc(filter.FromDate.Value.Date);
                query = query.Where(x => x.StartedAt >= fromUtc);
            }

            if (filter.ToDate.HasValue)
            {
                var toUtc = DateTimeHelper.ConvertClientLocalToUtc(filter.ToDate.Value.Date.AddDays(1));
                query = query.Where(x => x.StartedAt < toUtc);
            }

            var totalCount = await query.CountAsync();

            var isDescending =
                filter.SortDirection?.ToString().Equals("Descending", StringComparison.OrdinalIgnoreCase) == true ||
                filter.SortDirection?.ToString().Equals("Desc", StringComparison.OrdinalIgnoreCase) == true;

            query = isDescending
                ? query.OrderByDescending(x => x.StartedAt)
                : query.OrderBy(x => x.StartedAt);

            var logs = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<SystemJobLogDto>
            {
                Items = logs.Select(MapToDto).ToList(),
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<SystemJobLogDto?> GetLatestAsync(SystemJobName? jobName = null)
        {
            var query = _unitOfWork.Repository<SystemJobLog, Guid>()
                .Query()
                .AsQueryable();

            if (jobName.HasValue)
            {
                query = query.Where(x => x.JobName == jobName.Value);
            }

            var log = await query
                .OrderByDescending(x => x.StartedAt)
                .FirstOrDefaultAsync();

            return log == null ? null : MapToDto(log);
        }

        private async Task<SystemJobLog> GetTrackedJobLogAsync(Guid jobLogId)
        {
            var log = await _unitOfWork.Repository<SystemJobLog, Guid>()
                .Query(withTracking: true)
                .FirstOrDefaultAsync(x => x.Id == jobLogId);

            if (log == null)
                throw new KeyNotFoundException("System job log not found");

            return log;
        }

        private static SystemJobLogDto MapToDto(SystemJobLog log)
        {
            return new SystemJobLogDto
            {
                Id = log.Id,
                JobName = log.JobName,
                Status = log.Status,
                StartedAt = DateTimeHelper.EnsureUtcKind(log.StartedAt),
                FinishedAt = DateTimeHelper.EnsureUtcKind(log.FinishedAt),
                DurationMs = log.DurationMs,
                ProcessedCount = log.ProcessedCount,
                CreatedCount = log.CreatedCount,
                UpdatedCount = log.UpdatedCount,
                FailedCount = log.FailedCount,
                ErrorMessage = log.ErrorMessage,
                TriggeredBy = log.TriggeredBy,
                MetadataJson = log.MetadataJson,
                CreatedAt = DateTimeHelper.EnsureUtcKind(log.CreatedAt)
            };
        }
    }
}