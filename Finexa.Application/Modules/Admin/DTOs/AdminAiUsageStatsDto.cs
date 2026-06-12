namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminAiUsageStatsDto
    {
        public int TotalAiTransactions { get; set; }

        public int AiTransactionsThisMonth { get; set; }

        public List<AdminTransactionSourceStatsDto> SourceStats { get; set; } = new();
    }
}