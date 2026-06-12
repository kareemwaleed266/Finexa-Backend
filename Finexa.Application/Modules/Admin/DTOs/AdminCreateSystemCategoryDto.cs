using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminCreateSystemCategoryDto
    {
        public string Name { get; set; } = string.Empty;

        public TransactionType Type { get; set; }
    }
}