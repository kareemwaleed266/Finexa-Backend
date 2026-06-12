using Finexa.Application.Common.Files;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.AI.OCR.Interfaces;
using Finexa.Application.Modules.Transactions.DTOs;
using Finexa.Application.Modules.Transactions.Interfaces;
using Finexa.Domain.Entities;
using Finexa.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Finexa.Application.Modules.AI.OCR.Services
{
    public class OcrAppService : IOcrAppService
    {
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024;

        private static readonly string[] AllowedContentTypes =
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp"
        };

        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOcrService _ocrService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ITransactionService _transactionService;

        public OcrAppService(
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork,
            IOcrService ocrService,
            IFileStorageService fileStorageService,
            ITransactionService transactionService)
        {
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _ocrService = ocrService;
            _fileStorageService = fileStorageService;
            _transactionService = transactionService;
        }

        public async Task ProcessReceiptAsync(IFormFile file)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            ValidateImage(file);

            var ocrResult = await _ocrService.ProcessAsync(file);

            if (ocrResult == null)
                throw new Exception("OCR returned empty response");

            if (ocrResult.Amount <= 0)
                throw new Exception("Invalid transaction amount from OCR");

            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var categories = await categoryRepo
                .WhereAsync(c => c.AppUserId == null || c.AppUserId == userId);

            var receiptCategory = categories.FirstOrDefault(c =>
                c.AppUserId == null &&
                c.Name.Equals("Receipt", StringComparison.OrdinalIgnoreCase));

            if (receiptCategory == null)
                throw new Exception("Receipt category is missing. Please run category seeder.");

            var createDto = new CreateTransactionDto
            {
                Amount = ocrResult.Amount,
                CategoryId = receiptCategory.Id,
                Type = TransactionType.Expense,
                Notes = "Transaction Created From OCR",
                OccurredAt = ocrResult.OccurredAt,
                Merchant = ocrResult.Merchant,
                Item = ocrResult.Item
            };

            FileUploadResultDto uploadedFile;

            await using (var stream = file.OpenReadStream())
            {
                uploadedFile = await _fileStorageService.UploadImageAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    "finexa/receipts");
            }

            try
            {
                var transactionId = await _transactionService.AddTransactionAsync(createDto, TransactionSource.OCR, null);

                await _transactionService.AddTransactionAttachmentAsync(transactionId, uploadedFile, AttachmentType.Receipt);
            }
            catch
            {
                await _fileStorageService.DeleteAsync(uploadedFile.PublicId);
                throw;
            }
        }

        private static void ValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is required");

            if (file.Length > MaxFileSizeInBytes)
                throw new Exception("File size must not exceed 5MB");

            if (string.IsNullOrWhiteSpace(file.ContentType) ||
                !AllowedContentTypes.Contains(file.ContentType.ToLower()))
            {
                throw new Exception("Only JPG, PNG, and WEBP images are allowed");
            }
        }
    }
}