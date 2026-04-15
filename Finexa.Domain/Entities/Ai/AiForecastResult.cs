namespace Finexa.Domain.Entities.Ai
{
    public class AiForecastResult : BaseAuditableEntity<Guid>
    {
        public Guid AppUserId { get; set; }
        public virtual AppUser User { get; set; } = null!;

        public ForecastType ForecastType { get; set; } = ForecastType.Balance; // e.g. "Balance", "Expense"
        public decimal PredictedValue { get; set; }
        public DateTime ForecastForDate { get; set; } // e.g. next month
    }
}
