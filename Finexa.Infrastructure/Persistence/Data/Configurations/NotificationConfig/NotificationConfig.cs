using Finexa.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finexa.Infrastructure.Persistence.Data.Configurations.NotificationConfig
{
    public class NotificationConfig : BaseAuditableEntityConfig<Notification>
    {
        public override void Configure(EntityTypeBuilder<Notification> builder)
        {
            base.Configure(builder);

            builder.ToTable("Notifications");

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Severity)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ShouldToast)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.ReadAt)
                .IsRequired(false);

            builder.Property(x => x.RelatedEntityType)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(x => x.RelatedEntityId)
                .IsRequired(false);

            builder.Property(x => x.ActionUrl)
                .HasMaxLength(300)
                .IsRequired(false);

            builder.Property(x => x.DeduplicationKey)
                .HasMaxLength(250)
                .IsRequired(false);

            builder.HasOne(x => x.AppUser)
                .WithMany()
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.AppUserId);

            builder.HasIndex(x => new { x.AppUserId, x.IsRead });

            builder.HasIndex(x => x.CreatedAt);

            builder.HasIndex(x => new { x.AppUserId, x.DeduplicationKey })
                .IsUnique()
                .HasFilter("[DeduplicationKey] IS NOT NULL");
        }
    }
}