namespace Finexa.Domain.Entities.Identity
{
    public class RefreshToken : BaseAuditableEntity<Guid>
    {
        public string Token { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; } = false;

        public Guid AppUserId { get; set; }

        public AppUser User { get; set; } = null!;
    }
}