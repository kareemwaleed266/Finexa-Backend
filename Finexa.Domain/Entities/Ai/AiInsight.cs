
namespace Finexa.Domain.Entities.Ai
{
    public class AiInsight : BaseAuditableEntity<Guid>
    {
        public Guid AppUserId { get; set; }
        public virtual AppUser User { get; set; } = null!;

        public string Title { get; set; } = string.Empty; // e.g. "High Spending on Entertainment"
        public string Description { get; set; } = string.Empty; // Full text of the insight
        public AiInsightSource Source { get; set; } = AiInsightSource.ChatAgent; // e.g. "AI Model", "Chat Agent", etc.

        public virtual Category Category { get; set; } // e.g. "Spending", "Budget", "Goal"
        public Guid CategoryId { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
