using Finexa.Application.Common.DTOs;
using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Transactions.DTOs
{
    public class TransactionFilterDto : BaseFilterDto
    {
        public TransactionType? Type { get; set; }

        public Guid? CategoryId { get; set; }
        public PeriodType? Period { get; set; } = PeriodType.Month;

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public TransactionSortBy? SortBy { get; set; }

    }
}