namespace Finexa.Application.Modules.AI.ParseTransaction.DTOs
{
    public class ParseTransactionRequestDto
    {
        public string Text { get; set; } = default!;

        public List<string> AvailableCategories { get; set; } = new();
    }
}