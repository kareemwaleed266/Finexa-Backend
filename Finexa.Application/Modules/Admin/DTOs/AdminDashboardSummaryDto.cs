namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminDashboardSummaryDto
    {
        public AdminUserStatsDto Users { get; set; } = new();

        public AdminFinancialStatsDto Financial { get; set; } = new();

        public AdminBillsStatsDto Bills { get; set; } = new();

        public AdminAiUsageStatsDto AiUsage { get; set; } = new();

        public AdminJobHealthDto JobHealth { get; set; } = new();

        public List<AdminRecentAuditLogDto> RecentAuditLogs { get; set; } = new();
    }
}