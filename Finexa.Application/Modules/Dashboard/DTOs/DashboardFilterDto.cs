using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Dashboard.DTOs
{
    public class DashboardFilterDto
    {
        public PeriodType Period { get; set; } = PeriodType.Month;

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}