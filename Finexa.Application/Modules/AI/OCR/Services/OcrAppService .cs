using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.AI.OCR.DTOs;
using Finexa.Application.Modules.AI.OCR.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Finexa.Application.Modules.AI.OCR.Services
{
    public class OcrAppService : IOcrAppService
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOcrService _ocrService;

        public OcrAppService(
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork,
            IOcrService ocrService)
        {
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _ocrService = ocrService;
        }

        public async Task<ReceiptOcrResponseDto> ProcessReceiptAsync(IFormFile file)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            if (file == null || file.Length == 0)
                throw new Exception("File is required");

            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var categories = await categoryRepo
                .WhereAsync(c => c.AppUserId == null || c.AppUserId == userId);

            var categoryNames = categories
                .Select(c => c.Name)
                .ToList();

            var result = await _ocrService.ProcessAsync(file, categoryNames);

            return result;
        }
    }
}
