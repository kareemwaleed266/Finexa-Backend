using Finexa.Application.Modules.AI.ParseTransaction.DTOs;

namespace Finexa.Application.Modules.AI.ParseTransaction.Interfaces
{
    public interface IParseTransactionService
    {
        Task<ParseTransactionResponseDto> ParseAsync(ParseTransactionRequestDto dto);
    }
}