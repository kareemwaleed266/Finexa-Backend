namespace Finexa.Application.Modules.AI.ParseTransaction.DTOs
{
    public class ParsedTransactionItemDto
    {
        public decimal Amount { get; set; }

        public string? CategoryName { get; set; } 

        public string Type { get; set; } = default!;

        public string? Notes { get; set; }

        public DateTime? OccurredAt { get; set; }
        public string? Merchant { get; set; }
        public string? Item { get; set; }
    }
}
