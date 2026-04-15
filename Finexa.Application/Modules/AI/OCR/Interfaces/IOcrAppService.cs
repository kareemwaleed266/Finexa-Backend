using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Application.Modules.AI.OCR.DTOs;
using Microsoft.AspNetCore.Http;

namespace Finexa.Application.Modules.AI.OCR.Interfaces
{
    public interface IOcrAppService
    {
        Task<ReceiptOcrResponseDto> ProcessReceiptAsync(IFormFile file);
    }
}
