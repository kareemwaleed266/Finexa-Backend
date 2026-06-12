namespace Finexa.Application.Modules.Admin.DTOs
{
    public class AdminUserStatsDto
    {
        public int TotalUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int DeactivatedUsers { get; set; }

        public int LockedUsers { get; set; }

        public int NewUsersThisMonth { get; set; }
    }
}