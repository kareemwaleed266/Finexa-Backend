using Finexa.Application.Modules.AI.OCR.Interfaces;
using Finexa.Application.Modules.AI.ParseTransaction.DTOs;
using Finexa.Application.Modules.AI.ParseTransaction.Interfaces;
using Finexa.Application.Modules.Transactions.DTOs;
using Finexa.Application.Modules.Transactions.Interfaces;
using Finexa.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finexa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IParseTransactionAppService _parseAppService;
        private readonly IOcrAppService _ocrAppService;

        public TransactionController(ITransactionService transactionService, IParseTransactionAppService parseAppService, IOcrAppService ocrAppService)
        {
            _transactionService = transactionService;
            _parseAppService = parseAppService;
            _ocrAppService = ocrAppService;
        }

        [HttpPost("add-transaction")]
        public async Task<IActionResult> AddTransaction([FromBody] CreateTransactionDto dto)
        {
            await _transactionService.AddTransactionAsync(dto, TransactionSource.Manual, null);

            return Ok(new { message = "Transaction created successfully" });
        }

        /*[HttpPost("from-chat")]
        public async Task<IActionResult> AddFromChat(CreateTransactionDto dto)
        {
            await _transactionService.AddTransactionAsync(dto, TransactionSource.Chat, null);

            return Ok(new { message = "Transaction created from chat" });
        }
        */

        //[HttpPost("from-ocr")]
        //public async Task<IActionResult> AddFromOcr(
        //                [FromBody] List<ConfirmParsedTransactionDto> dtos)
        //{
        //    var createDtos = await _transactionService
        //        .ConfirmTransactionsAsync(dtos);

        //    foreach (var dto in createDtos)
        //    {
        //        await _transactionService.AddTransactionAsync(
        //            dto,
        //            TransactionSource.OCR
        //        );
        //    }                 

        //    return Ok(new { message = "Transaction created from OCR" });
        //}

        //[HttpPost("from-speech")]
        //public async Task<IActionResult> AddFromSpeech(CreateTransactionDto dto)
        //{
        //    await _transactionService.AddTransactionAsync(dto, TransactionSource.Speech, null);

        //    return Ok(new { message = "Transactions created successfully" });
        //}

        [HttpPost("from-speech")]
        public async Task<IActionResult> AddFromSpeech([FromBody] ParseRequestDto request)
        {
            var parsed = await _parseAppService.ParseAsync(request.Text);

            if (parsed.Transactions == null || !parsed.Transactions.Any())
            {
                return Ok(new
                {
                    message = "No transactions detected",
                    count = 0
                });
            }

            var createdCount = await _transactionService.AddParsedTransactionsAsync(
                parsed.Transactions,
                TransactionSource.Speech);

            return Ok(new
            {
                message = "Transactions created successfully",
                count = createdCount
            });
        }

        [HttpPost("from-ocr")]
        public async Task<IActionResult> ProcessReceipt(IFormFile file)
        {
            await _ocrAppService.ProcessReceiptAsync(file);

            return Ok(new
            {
                message = "Transaction created from OCR successfully"
            });
        }
        //[HttpPost("confirm-transaction")]
        //public async Task<IActionResult> ConfirmTransaction([FromBody] ConfirmParsedTransactionDto dto)
        //{
        //    var result = await _transactionService.ConfirmTransactionsAsync(dto);

        //    return Ok(new
        //    {
        //        message = "Transaction is valid",
        //        data = result
        //    });
        //}


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(Guid id, UpdateTransactionDto dto)
        {
            await _transactionService.UpdateTransactionAsync(id, dto);

            return Ok(new { message = "Transaction updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(Guid id)
        {
            await _transactionService.DeleteTransactionAsync(id);

            return Ok(new { message = "Transaction deleted successfully" });
        }

        [HttpPost("adjust-balance")]
        public async Task<IActionResult> AdjustBalance(AdjustBalanceDto dto)
        {
            await _transactionService.AdjustBalanceAsync(dto);

            return Ok(new { message = "Balance adjusted successfully" });
        }

        [HttpGet("get-transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] TransactionFilterDto filter)
        {
            var result = await _transactionService.GetTransactionsAsync(filter);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionById(Guid id)
        {
            var result = await _transactionService.GetTransactionByIdAsync(id);

            return Ok(result);
        }
    }
}