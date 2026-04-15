
namespace Finexa.Domain.Entities.Identity
{
    public class AppUser : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }

        public DateTime? LastLoginAt { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string? ProfileImageUrl { get; set; }
        //public bool IsActive { get; set; } = true;

        public virtual UserBalance? UserBalance { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
        //public virtual ICollection<SavingPlan> SavingPlans { get; set; } = new List<SavingPlan>();
    }
}