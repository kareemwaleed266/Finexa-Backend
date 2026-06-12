namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminFinancialStatsDto
    {
        public int TotalTransactions { get; set; }

        public int TransactionsThisMonth { get; set; }

        public decimal TotalIncome { get; set; }

        public decimal TotalExpense { get; set; }

        public decimal TotalBalance { get; set; }

        public int TotalGoals { get; set; }

        public int GoalsCreatedThisMonth { get; set; }
    }
}