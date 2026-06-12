using Finexa.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finexa.Infrastructure.Persistence.Data.Configurations.BillConfig
{
    public class BillOccurrenceConfig : BaseAuditableEntityConfig<BillOccurrence>
    {
        public override void Configure(EntityTypeBuilder<BillOccurrence> builder)
        {
            base.Configure(builder);

            builder.Property(o => o.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(o => o.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.DueDate)
                .IsRequired();

            builder.Property(o => o.PeriodStart)
                .IsRequired();

            builder.Property(o => o.PeriodEnd)
                .IsRequired();

            builder.Property(o => o.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(o => o.OccurrenceType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(o => o.IsGeneratedAutomatically)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(o => o.PaidAt)
                .IsRequired(false);

            builder.Property(o => o.Notes)
                .HasMaxLength(500);

            builder.HasOne(o => o.AppUser)
                .WithMany()
                .HasForeignKey(o => o.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.BillSeries)
                .WithMany(b => b.Occurrences)
                .HasForeignKey(o => o.BillSeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.Payments)
                .WithOne(p => p.BillOccurrence)
                .HasForeignKey(p => p.BillOccurrenceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(o => o.AppUserId);
            builder.HasIndex(o => o.BillSeriesId);
            builder.HasIndex(o => o.DueDate);
            builder.HasIndex(o => o.Status);

            builder.HasIndex(o => new
            {
                o.BillSeriesId,
                o.DueDate,
                o.OccurrenceType
            });
        }
    }
}