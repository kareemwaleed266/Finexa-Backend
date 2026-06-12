using Finexa.Domain.Entities.Admin;
using Finexa.Domain.Entities.Ai.Chat;
using Finexa.Domain.Entities.Financial;
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
        modelBuilder.Entity<AppUser>()
            .Property(x => x.IsActive)
            .HasDefaultValue(true);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FinexaDbContext).Assembly);
    }
    // Identity
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // Financial
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<UserBalance> UserBalances { get; set; }
    public DbSet<TransactionAttachment> TransactionAttachments { get; set; }


    public DbSet<SavingPlan> SavingPlans { get; set; }
    public DbSet<SavingPlanItem> SavingPlanItems { get; set; }
    public DbSet<SavingPlanMonthlyProgress> SavingPlanMonthlyProgress { get; set; }

    // AI Chat
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    // bills
    public DbSet<BillSeries> BillSeries { get; set; }
    public DbSet<BillOccurrence> BillOccurrences { get; set; }
    public DbSet<BillPayment> BillPayments { get; set; }

    // Admin
    public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }
    public DbSet<SystemJobLog> SystemJobLogs { get; set; }
}