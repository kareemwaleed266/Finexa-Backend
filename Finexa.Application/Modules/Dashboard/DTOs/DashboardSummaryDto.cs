namespace Finexa.Application.Modules.Dashboard.DTOs
{
    public class DashboardSummaryDto
    {
        public decimal TotalBalance { get; set; }     //  UserBalance
        public decimal TotalIncome { get; set; }      
        public decimal TotalExpense { get; set; }    
        public decimal TotalSavings { get; set; }     // Income - Expense

        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public ChangeDto? IncomeChangePercentage { get; set; }
        public ChangeDto? ExpenseChangePercentage { get; set; }
        public ChangeDto? SavingsChangePercentage { get; set; }


        public List<CategoryBreakdownDto> ExpenseBreakdown { get; set; } = new();

        public List<MoneyFlowDto> MoneyFlow { get; set; } = new();

        //public List<RecentTransactionDto> RecentTransactions { get; set; } = new();
        //public List<GoalProgressDto> Goals { get; set; } = new();
    }
}