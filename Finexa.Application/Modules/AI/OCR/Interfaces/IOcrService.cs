using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Application.Modules.AI.OCR.DTOs;
using Microsoft.AspNetCore.Http;

namespace Finexa.Application.Modules.AI.OCR.Interfaces
{
    public interface IOcrService
    {
        Task<ReceiptOcrResponseDto> ProcessAsync(
            IFormFile file,
            List<string> availableCategories);
    }
}
