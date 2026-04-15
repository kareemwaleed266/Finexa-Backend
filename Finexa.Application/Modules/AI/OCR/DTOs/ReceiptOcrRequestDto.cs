using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Finexa.Application.Modules.AI.OCR.DTOs
{
    public class ReceiptOcrRequestDto
    {
        public IFormFile File { get; set; } = default!;
    }
}
