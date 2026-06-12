using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Transactions.DTOs
{
    public class TransactionDto
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string? Notes { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? Merchant { get; set; }
        public string? Item { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public TransactionSource Source { get; set; }


        public bool HasReceipt { get; set; }
        public string? ReceiptImageUrl { get; set; }
    }
}