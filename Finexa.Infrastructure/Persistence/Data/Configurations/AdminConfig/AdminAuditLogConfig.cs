using Finexa.Domain.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finexa.Infrastructure.Persistence.Configurations.Admin
{
    public class AdminAuditLogConfig : BaseAuditableEntityConfig<AdminAuditLog>
    {
        public override void Configure(EntityTypeBuilder<AdminAuditLog> builder)
        {
            base.Configure(builder);

            builder.ToTable("AdminAuditLogs");

            builder.Property(x => x.AdminEmail)
                .HasMaxLength(256);

            builder.Property(x => x.Action)
                .IsRequired();

            builder.Property(x => x.TargetType)
                .IsRequired();

            builder.Property(x => x.TargetDisplayName)
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Reason)
                .HasMaxLength(500);

            builder.Property(x => x.IpAddress)
                .HasMaxLength(100);

            builder.Property(x => x.UserAgent)
                .HasMaxLength(500);

            builder.Property(x => x.OldValuesJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.NewValuesJson)
                .HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.AdminUser)
                .WithMany()
                .HasForeignKey(x => x.AdminUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.AdminUserId);

            builder.HasIndex(x => x.Action);

            builder.HasIndex(x => x.TargetType);

            builder.HasIndex(x => x.TargetId);

            builder.HasIndex(x => x.CreatedAt);
        }
    }
}