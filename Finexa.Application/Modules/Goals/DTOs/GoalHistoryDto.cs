using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Goals.DTOs
{
    public class GoalHistoryDto
    {
        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public string? Notes { get; set; }
    }
}