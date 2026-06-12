namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminUserDetailsDto
    {
        public Guid Id { get; set; }

        public string? Email { get; set; }

        public string? UserName { get; set; }

        public string? PhoneNumber { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool IsActive { get; set; }

        public DateTime? DeactivatedAt { get; set; }

        public string? DeactivationReason { get; set; }

        public bool IsLocked { get; set; }

        public DateTime? LockoutEnd { get; set; }

        public int AccessFailedCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<string> Roles { get; set; } = new();

        public int TransactionsCount { get; set; }

        public int GoalsCount { get; set; }

        public int BillSeriesCount { get; set; }

        public int BillOccurrencesCount { get; set; }

        public int PaidBillOccurrencesCount { get; set; }

        public int OverdueBillOccurrencesCount { get; set; }

        public decimal TotalIncome { get; set; }

        public decimal TotalExpense { get; set; }

        public decimal TotalBalance { get; set; }
    }
}