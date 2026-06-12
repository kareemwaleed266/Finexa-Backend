using Finexa.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finexa.Infrastructure.Persistence.Data.Configurations.SavingPlanConfig
{
    public class SavingPlanConfig : BaseAuditableEntityConfig<SavingPlan>
    {
        public override void Configure(EntityTypeBuilder<SavingPlan> builder)
        {
            base.Configure(builder);

            builder.ToTable("SavingPlans");

            builder.Property(x => x.AnalysisPeriodMonths)
                .IsRequired();
            builder.Property(x => x.InsightsJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property(x => x.WarningsJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);
            builder.Property(x => x.PlanType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.TargetMonthlySaving)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.StartDate)
                .IsRequired();

            builder.Property(x => x.EndDate)
                .IsRequired(false);

            builder.Property(x => x.AverageIncome)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.AverageExpenses)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.CurrentAverageSaving)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ForecastedIncome)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ForecastedExpenses)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ForecastedSaving)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.RecommendedMonthlySaving)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ExtraSavingOpportunity)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Difficulty)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.PlanStatusLabel)
                .HasMaxLength(50);

            builder.Property(x => x.SummaryMessage)
                .HasMaxLength(1000);

            builder.Property(x => x.AppliedAt)
                .IsRequired(false);

            builder.Property(x => x.DeactivatedAt)
                .IsRequired(false);

            builder.HasOne(x => x.AppUser)
                .WithMany()
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.SavingPlan)
                .HasForeignKey(x => x.SavingPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.MonthlyProgress)
                .WithOne(x => x.SavingPlan)
                .HasForeignKey(x => x.SavingPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.AppUserId);

            builder.HasIndex(x => new { x.AppUserId, x.Status })
                .IsUnique()
                .HasFilter("[Status] = 1");
        }
    }
}