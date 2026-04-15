namespace Finexa.Application.Modules.AI.ParseTransaction.DTOs
{
    public class ParseTransactionResponseDto
    {
        public List<ParsedTransactionItemDto> Transactions { get; set; } = new();
    }
}