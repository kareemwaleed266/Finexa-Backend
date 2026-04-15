//namespace Finexa.Domain.Entities.Financial
//{
//    public class SavingPlan : BaseAuditableEntity<Guid>
//    {
//        public string Title { get; set; } = string.Empty;

//        public decimal LimitAmount { get; set; }

//        public DateTime StartDate { get; set; }

//        public DateTime EndDate { get; set; }

//        public virtual AppUser AppUser { get; set; } = null!;
//        public Guid AppUserId { get; set; }

//        public virtual ICollection<SavingPlanCategory> Categories { get; set; }
//            = new List<SavingPlanCategory>();

//        public bool IsActive()
//        {
//            var now = DateTime.UtcNow;
//            return now >= StartDate && now <= EndDate;
//        }
//    }
//}           