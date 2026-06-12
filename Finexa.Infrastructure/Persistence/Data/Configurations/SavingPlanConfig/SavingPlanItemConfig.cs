using Finexa.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finexa.Infrastructure.Persistence.Data.Configurations.SavingPlanConfig
{
    public class SavingPlanItemConfig : BaseAuditableEntityConfig<SavingPlanItem>
    {
        public override void Configure(EntityTypeBuilder<SavingPlanItem> builder)
        {
            base.Configure(builder);

            builder.ToTable("SavingPlanItems");

            builder.Property(x => x.CategoryName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.CategoryType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.CurrentAverage)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.RecommendedBudget)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ReductionPercentage)
                .HasColumnType("decimal(5,2)");

            builder.Property(x => x.ExpectedSaving)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Reason)
                .HasMaxLength(1000);

            builder.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasIndex(x => x.SavingPlanId);

            builder.HasIndex(x => x.CategoryId);
        }
    }
}