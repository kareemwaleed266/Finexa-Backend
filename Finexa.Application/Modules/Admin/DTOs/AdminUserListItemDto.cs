namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminUserListItemDto
    {
        public Guid Id { get; set; }

        public string? Email { get; set; }

        public string? UserName { get; set; }

        public string? PhoneNumber { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool IsActive { get; set; }

        public bool IsLocked { get; set; }

        public DateTime? LockoutEnd { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<string> Roles { get; set; } = new();

        public int TransactionsCount { get; set; }

        public int GoalsCount { get; set; }

        public int BillsCount { get; set; }
    }
}