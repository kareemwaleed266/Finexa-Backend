public class Category : BaseAuditableEntity<Guid>
{
    public string Name { get; set; } = null!;

    public TransactionType Type { get; set; } // Income / Expense

    public bool IsDefault { get; set; } = false;

    public Guid? AppUserId { get; set; }  
    public virtual AppUser? AppUser { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; }
        = new List<Transaction>();
}