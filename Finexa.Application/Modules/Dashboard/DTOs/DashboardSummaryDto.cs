namespace Finexa.Application.Modules.Dashboard.DTOs
{
    public class DashboardSummaryDto
    {
        public decimal TotalBalance { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }

        public List<RecentTransactionDto> RecentTransactions { get; set; } = new();
        public List<GoalProgressDto> Goals { get; set; } = new();
    }
}