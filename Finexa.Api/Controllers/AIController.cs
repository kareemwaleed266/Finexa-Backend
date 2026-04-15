using Finexa.Application.Modules.AI.OCR.Interfaces;
using Finexa.Application.Modules.AI.ParseTransaction.DTOs;
using Finexa.Application.Modules.AI.ParseTransaction.Interfaces;
using Finexa.Application.Modules.AI.STT.Interfaces;
using Finexa.Application.Modules.AI.STT.Services;
using Finexa.Application.Modules.Transactions.DTOs;
using Finexa.Application.Modules.Transactions.Interfaces;
using Finexa.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class AIController : ControllerBase
    {
       private readonly ISpeechAppService _speechAppService;
       private readonly IParseTransactionAppService _parseAppService;
       private readonly ITransactionService _transactionService;
       private readonly IOcrAppService _ocrAppService;
       private readonly IUnitOfWork _unitOfWork;
        public AIController(
            ISpeechAppService speechAppService,
            IParseTransactionAppService parseAppService,
            ITransactionService transactionService,IUnitOfWork unitOfWork, IOcrAppService ocrAppService)
        {
            _speechAppService = speechAppService;
            _parseAppService = parseAppService;
            _transactionService = transactionService;
            _unitOfWork = unitOfWork;
            _ocrAppService = ocrAppService;
        }

        [HttpPost("voice-to-text")]
        public async Task<IActionResult> VoiceToText(IFormFile file)
        {
            var text = await _speechAppService.ConvertSpeechAsync(file);

            return Ok(new { text });
        }

        //[HttpPost("parse-and-create")]
        //public async Task<IActionResult> ParseAndCreate([FromBody] ParseRequestDto request)
        //{
        //    var parsed = await _parseAppService.ParseAsync(request.Text);

        //    var transaction = parsed.Transactions.First();
        //    var categories =  _unitOfWork.Repository<Category, Guid>();
        //    var category = await categories.FirstOrDefaultAsync(c => c.Name == transaction.Category);

        //    var dto = new CreateTransactionDto
        //    {
        //        Amount = transaction.Amount,
        //        Type = transaction.Type?.ToLower() == "income"
        //            ? TransactionType.Income
        //            : TransactionType.Expense,
        //        CategoryId = category.Id,
        //        Notes = transaction.Category ?? "AI Transaction",
        //        OccurredAt = transaction.OccurredAt ?? DateTime.UtcNow
        //    };
        //    await _transactionService.AddTransactionAsync(dto, TransactionSource.Speech);

        //    return Ok(new { message = "Transaction created successfully" });
        //}

        [HttpPost("send-receipt-ocr")]
        public async Task<IActionResult> ProcessReceipt(IFormFile file)
        {
            var result = await _ocrAppService.ProcessReceiptAsync(file);

            return Ok(result);
        }
    }
}
