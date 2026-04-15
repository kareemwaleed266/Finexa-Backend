public class Transaction : BaseAuditableEntity<Guid>
{
    public decimal Amount { get; set; }

    public TransactionType Type { get; set; } // Income / Expense

    public string? Notes { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = default!;

    public Guid AppUserId { get; set; }
    public virtual AppUser AppUser { get; set; } = default!;

    public TransactionSource Source { get; set; } = TransactionSource.Manual;

    public Guid? GoalId { get; set; }
    public virtual Goal? Goal { get; set; }
}