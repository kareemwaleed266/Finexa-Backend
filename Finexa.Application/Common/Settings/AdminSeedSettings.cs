namespace Finexa.Application.Common.Settings
{
    public class AdminSeedSettings
    {
        public bool Enabled { get; set; } = true;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
    }
}