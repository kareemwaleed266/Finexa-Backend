using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminTransactionSourceStatsDto
    {
        public TransactionSource Source { get; set; }

        public int Count { get; set; }

        public decimal TotalAmount { get; set; }
    }
}