using Finexa.Application.Modules.Admin.DTOs;

namespace Finexa.Application.Modules.Admin.Interfaces
{
    public interface IAdminAiMonitoringService
    {
        Task<AdminAiUsageSummaryDto> GetUsageSummaryAsync();
    }
}