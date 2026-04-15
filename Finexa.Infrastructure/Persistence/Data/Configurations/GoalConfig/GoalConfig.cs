using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GoalConfig : BaseAuditableEntityConfig<Goal>
{
    public override void Configure(EntityTypeBuilder<Goal> builder)
    {
        base.Configure(builder);

        builder.Property(t => t.CurrentAmount)
           .HasColumnType("decimal(18,2)")
           .IsRequired();

        builder.Property(t => t.TargetAmount)
           .HasColumnType("decimal(18,2)")
           .IsRequired();

        builder.Property(g => g.Status)
           .HasConversion<int>();

        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Description)
            .HasMaxLength(500);

        builder.HasOne(g => g.AppUser)
            .WithMany(u => u.Goals)
            .HasForeignKey(g => g.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}