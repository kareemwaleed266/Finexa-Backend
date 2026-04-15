namespace Finexa.Infrastructure.Persistence.Repositories
{
    public class FinexaContextInitializer(FinexaDbContext _context) : IFinexaContextInitializer
    {
        public async Task InitializeAsync()
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
                await _context.Database.MigrateAsync();
        }
    }
}
