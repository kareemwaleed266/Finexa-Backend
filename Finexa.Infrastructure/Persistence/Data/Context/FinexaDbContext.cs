using Finexa.Domain.Entities.Ai.Chat;
using Finexa.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class FinexaDbContext
    : IdentityDbContext<AppUser, AppRole, Guid>
{
    public FinexaDbContext(DbContextOptions<FinexaDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FinexaDbContext).Assembly);
    }

    // Financial
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<UserBalance> UserBalances { get; set; }


    // AI Chat
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
}