using Finexa.Application.Modules.AI.OCR.DTOs;
using Microsoft.AspNetCore.Http;

namespace Finexa.Application.Modules.AI.OCR.Interfaces
{
    public interface IOcrAppService
    {
        Task ProcessReceiptAsync(IFormFile file);
    }
}