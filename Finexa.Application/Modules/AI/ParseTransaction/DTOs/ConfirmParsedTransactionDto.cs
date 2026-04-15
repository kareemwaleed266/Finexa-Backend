using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Application.Modules.AI.ParseTransaction.DTOs
{
    public class ConfirmParsedTransactionDto
    {
        public decimal Amount { get; set; }

        public Guid CategoryId { get; set; }

        public string Type { get; set; } = default!;

        public string? Notes { get; set; }

        public DateTime? OccurredAt { get; set; }
    }
}
