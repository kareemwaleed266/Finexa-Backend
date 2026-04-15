using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finexa.Application.Modules.AI.OCR.DTOs
{
    public class ReceiptOcrResponseDto
    {
        public string? Merchant { get; set; }

        public decimal Total { get; set; }

        public string? Currency { get; set; }

        public DateTime? IssuedAt { get; set; }

        public string? CategoryName { get; set; } 
    }
}
