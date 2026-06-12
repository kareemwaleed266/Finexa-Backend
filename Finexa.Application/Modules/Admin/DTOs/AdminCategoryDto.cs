using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminCategoryDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public TransactionType Type { get; set; }

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; }

        public bool IsProtected { get; set; }

        public int TransactionsCount { get; set; }

        public int BillSeriesCount { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastModifiedAt { get; set; }
    }
}