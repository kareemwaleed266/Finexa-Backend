public class UserBalance : BaseAuditableEntity<Guid>
{
    public Guid AppUserId { get; set; }
    public virtual AppUser AppUser { get; set; }

    public decimal TotalIncome { get; set; } = 0;
    public decimal TotalExpense { get; set; } = 0;
    public decimal TotalBalance { get; set; } = 0;
}