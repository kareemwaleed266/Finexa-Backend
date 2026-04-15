using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Transactions.DTOs
{
    public class UpdateTransactionDto
    {
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public Guid CategoryId { get; set; }

        public string? Notes { get; set; }

        public DateTime? OccurredAt { get; set; }
        //public TransactionSource Source { get; set; } = TransactionSource.Manual;

    }
}