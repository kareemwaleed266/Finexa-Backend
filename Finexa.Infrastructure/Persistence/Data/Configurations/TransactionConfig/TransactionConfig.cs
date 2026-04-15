using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TransactionConfig : BaseAuditableEntityConfig<Transaction>
{
    public override void Configure(EntityTypeBuilder<Transaction> builder)
    {
        base.Configure(builder);

        builder.Property(t => t.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.Type)
            .HasConversion<int>();

        builder.Property(t => t.Source)
            .HasConversion<int>();

        builder.Property(t => t.OccurredAt)
            .IsRequired();

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        // User
        builder.HasOne(t => t.AppUser)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.AppUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Category
        builder.HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Goal (Optional)
        builder.HasOne(t => t.Goal)
            .WithMany(g => g.Transactions)
            .HasForeignKey(t => t.GoalId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(t => t.AppUserId);
        builder.HasIndex(t => t.CategoryId);
        builder.HasIndex(t => t.GoalId);
    }
}