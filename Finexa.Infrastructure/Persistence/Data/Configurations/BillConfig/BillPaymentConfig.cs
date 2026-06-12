using Finexa.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finexa.Infrastructure.Persistence.Data.Configurations.BillConfig
{
    public class BillPaymentConfig : BaseAuditableEntityConfig<BillPayment>
    {
        public override void Configure(EntityTypeBuilder<BillPayment> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.AmountPaid)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.PaidAt)
                .IsRequired();

            builder.Property(p => p.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(p => p.ReversedAt)
                .IsRequired(false);

            builder.Property(p => p.Notes)
                .HasMaxLength(500);

            builder.HasOne(p => p.AppUser)
                .WithMany()
                .HasForeignKey(p => p.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.BillSeries)
                .WithMany()
                .HasForeignKey(p => p.BillSeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.BillOccurrence)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.BillOccurrenceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Transaction)
                .WithMany()
                .HasForeignKey(p => p.TransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.ReversalTransaction)
                .WithMany()
                .HasForeignKey(p => p.ReversalTransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.AppUserId);
            builder.HasIndex(p => p.BillSeriesId);
            builder.HasIndex(p => p.BillOccurrenceId);

            builder.HasIndex(p => p.TransactionId)
                .IsUnique();

            builder.HasIndex(p => p.ReversalTransactionId)
                .IsUnique()
                .HasFilter("[ReversalTransactionId] IS NOT NULL");

            builder.HasIndex(p => p.BillOccurrenceId)
                .IsUnique()
                .HasFilter("[Status] = 1");
        }
    }
}