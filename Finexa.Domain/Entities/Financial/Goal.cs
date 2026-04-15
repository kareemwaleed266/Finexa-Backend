public class Goal : BaseAuditableEntity<Guid>
{
    public string Title { get; set; } = string.Empty;

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; private set; }

    public DateTime TargetDate { get; set; }

    public GoalStatus Status { get; private set; } = GoalStatus.InProgress;

    public Guid AppUserId { get; set; }
    public virtual AppUser AppUser { get; set; } = null!;

    public string? Description { get; set; }
    public bool IsRefunded { get; private set; }
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public void AddContribution(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Contribution must be positive");

        CurrentAmount += amount;

        if (CurrentAmount >= TargetAmount)
        {
            CurrentAmount = TargetAmount;
            Status = GoalStatus.Completed;
        }
    }

    public decimal GetRemainingAmount()
    {
        return TargetAmount - CurrentAmount;
    }

    public bool IsCompleted()
    {
        return Status == GoalStatus.Completed;
    }
    public bool IsCanceled()
    {
        return Status == GoalStatus.Canceled;
    }
    public void CancelGoal()
    {
        if (Status == GoalStatus.Completed)
            throw new InvalidOperationException("Completed goal cannot be canceled");

        Status = GoalStatus.Canceled;
    }

    public void ResetAfterRefund()
    {
        if (IsRefunded)
            throw new InvalidOperationException("Goal already refunded");

        CurrentAmount = 0;
        Status = GoalStatus.Canceled;
        IsRefunded = true;
    }
}