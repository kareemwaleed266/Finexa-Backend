namespace Finexa.Application.Modules.Identity.DTOs
{
    public class AuthResponseDto
    {
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime ExpireAt { get; set; }
    }
}