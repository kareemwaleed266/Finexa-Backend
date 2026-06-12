using Finexa.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finexa.Infrastructure.Persistence.Data.Configurations.BillConfig
{
    public class BillSeriesConfig : BaseAuditableEntityConfig<BillSeries>
    {
        public override void Configure(EntityTypeBuilder<BillSeries> builder)
        {
            base.Configure(builder);

            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(150);
            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.DeletedAt)
                .IsRequired(false);

            builder.HasIndex(x => new { x.AppUserId, x.IsDeleted });
            builder.Property(b => b.Description)
                .HasMaxLength(500);

            builder.Property(b => b.DefaultAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(b => b.AmountType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(b => b.Frequency)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(b => b.StartDate)
                .IsRequired();

            builder.Property(b => b.EndDate)
                .IsRequired(false);

            builder.Property(b => b.DueDay)
                .IsRequired(false);

            builder.Property(b => b.DueDate)
                .IsRequired(false);

            builder.Property(b => b.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(b => b.ReminderDaysBefore)
                .IsRequired()
                .HasDefaultValue(3);

            builder.Property(b => b.AllowsEarlyRenewal)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(b => b.AllowsTopUp)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(b => b.AppUser)
                .WithMany()
                .HasForeignKey(b => b.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Category)
                .WithMany()
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(b => b.Occurrences)
                .WithOne(o => o.BillSeries)
                .HasForeignKey(o => o.BillSeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(b => b.AppUserId);
            builder.HasIndex(b => b.CategoryId);
            builder.HasIndex(b => b.IsActive);
        }
    }
}