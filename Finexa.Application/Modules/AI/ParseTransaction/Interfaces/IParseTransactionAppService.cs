using Finexa.Application.Modules.AI.ParseTransaction.DTOs;

namespace Finexa.Application.Modules.AI.ParseTransaction.Interfaces
{
    public interface IParseTransactionAppService
    {
        Task<ParseTransactionResponseDto> ParseAsync(string text);
    }
}