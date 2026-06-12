using Finexa.Application.Common.Models;
using Finexa.Application.Modules.Admin.DTOs;
using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Admin.Interfaces
{
    public interface IAdminJobLogService
    {
        Task<Guid> StartJobAsync(
            SystemJobName jobName,
            string triggeredBy = "System",
            string? metadataJson = null);

        Task MarkJobSucceededAsync(
            Guid jobLogId,
            int processedCount = 0,
            int createdCount = 0,
            int updatedCount = 0,
            string? metadataJson = null);

        Task MarkJobFailedAsync(
            Guid jobLogId,
            string errorMessage,
            int processedCount = 0,
            int createdCount = 0,
            int updatedCount = 0,
            int failedCount = 1,
            string? metadataJson = null);

        Task<PagedResult<SystemJobLogDto>> GetLogsAsync(SystemJobLogFilterDto filter);

        Task<SystemJobLogDto?> GetLatestAsync(SystemJobName? jobName = null);
    }
}