using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Categories.DTOs
{
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TransactionType CategoryType { get; set; }
        public bool IsBillCategory { get; set; }
    }
}