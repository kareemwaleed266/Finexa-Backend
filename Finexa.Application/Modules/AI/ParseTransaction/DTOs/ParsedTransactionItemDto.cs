namespace Finexa.Application.Modules.AI.ParseTransaction.DTOs
{
    public class ParsedTransactionItemDto
    {
        public decimal Amount { get; set; }

        public string? Currency { get; set; }
        public string? CategoryName { get; set; } // 👈 اسم فقط

        public string Type { get; set; } = default!; // "income" | "expense"

        public string? Notes { get; set; }

        public DateTime? OccurredAt { get; set; }
    }
}
