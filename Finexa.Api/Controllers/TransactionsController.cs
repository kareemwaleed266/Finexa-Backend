using Finexa.Application.Modules.AI.ParseTransaction.DTOs;
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

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("add-transaction")]
        public async Task<IActionResult> AddTransaction([FromBody]CreateTransactionDto dto)
        {
            await _transactionService.AddTransactionAsync(dto, TransactionSource.Manual);

            return Ok(new { message = "Transaction created successfully" });
        }

        [HttpPost("from-chat")]
        public async Task<IActionResult> AddFromChat(CreateTransactionDto dto)
        {
            await _transactionService.AddTransactionAsync(dto, TransactionSource.Chat);

            return Ok(new { message = "Transaction created from chat" });
        }

        [HttpPost("from-ocr")]
        public async Task<IActionResult> AddFromOcr(
                        [FromBody] List<ConfirmParsedTransactionDto> dtos)
        {
            var createDtos = await _transactionService
                .ConfirmTransactionsAsync(dtos);

            foreach (var dto in createDtos)
            {
                await _transactionService.AddTransactionAsync(
                    dto,
                    TransactionSource.OCR
                );
            }

            return Ok(new { message = "Transaction created from OCR" });
        }

        [HttpPost("from-speech")]
        public async Task<IActionResult> AddFromSpeech(List<ConfirmParsedTransactionDto> dtos)
        {

            var createDtos = await _transactionService
                .ConfirmTransactionsAsync(dtos);

            foreach (var dto in createDtos)
            {
                await _transactionService.AddTransactionAsync(
                    dto,
                    TransactionSource.Speech
                );
            }

            return Ok(new { message = "Transactions created successfully" });
        }

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