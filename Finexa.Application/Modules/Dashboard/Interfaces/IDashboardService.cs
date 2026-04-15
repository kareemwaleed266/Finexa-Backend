using Finexa.Application.Modules.Dashboard.DTOs;

namespace Finexa.Application.Modules.Dashboard.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardAsync();
        Task RebuildBalanceAsync();
    }
}