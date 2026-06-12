using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Application.Modules.AI.OCR.DTOs
{
    public class ReceiptOcrResponseDto
    {
        public decimal Amount { get; set; }

        public string? CategoryName { get; set; }

        public string Type { get; set; } = "Expense";

        public string? Notes { get; set; }

        public DateTime? OccurredAt { get; set; }

        public string? Merchant { get; set; }

        public string? Item { get; set; }
    }
}
