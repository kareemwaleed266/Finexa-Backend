using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Transactions.DTOs
{
    public class CreateTransactionDto
    {
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }
        public Guid CategoryId { get; set; }

        public string? Notes { get; set; } = string.Empty;

        public DateTime? OccurredAt { get; set; }
        public Guid? GoalId { get; set; }
    }
}