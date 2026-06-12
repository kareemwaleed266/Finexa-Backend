namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminAiUsageSummaryDto
    {
        public int TotalTransactions { get; set; }

        public int TotalAiTransactions { get; set; }

        public int AiTransactionsThisMonth { get; set; }

        public decimal AiTransactionsAmount { get; set; }

        public List<AdminTransactionSourceStatsDto> SourceStats { get; set; } = new();
    }
}