using Finexa.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finexa.Infrastructure.Persistence.Data.Configurations.SavingPlanConfig
{
    public class SavingPlanMonthlyProgressConfig
        : BaseAuditableEntityConfig<SavingPlanMonthlyProgress>
    {
        public override void Configure(EntityTypeBuilder<SavingPlanMonthlyProgress> builder)
        {
            base.Configure(builder);

            builder.ToTable("SavingPlanMonthlyProgress");

            builder.Property(x => x.Year)
                .IsRequired();

            builder.Property(x => x.Month)
                .IsRequired();

            builder.Property(x => x.RecommendedMonthlySaving)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ActualIncome)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ActualExpenses)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ActualSaving)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Difference)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ProgressPercentage)
                .HasColumnType("decimal(5,2)");

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Summary)
                .HasMaxLength(1000);

            builder.Property(x => x.CalculatedAt)
                .IsRequired();

            builder.HasIndex(x => new { x.SavingPlanId, x.Year, x.Month })
                .IsUnique();
        }
    }
}