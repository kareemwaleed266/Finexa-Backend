using Finexa.Domain.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finexa.Infrastructure.Persistence.Configurations.Admin
{
    public class SystemJobLogConfig : BaseAuditableEntityConfig<SystemJobLog>
    {
        public override void Configure(EntityTypeBuilder<SystemJobLog> builder)
        {
            base.Configure(builder);

            builder.ToTable("SystemJobLogs");

            builder.Property(x => x.JobName)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.StartedAt)
                .IsRequired();

            builder.Property(x => x.ErrorMessage)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.TriggeredBy)
                .HasMaxLength(100);

            builder.Property(x => x.MetadataJson)
                .HasColumnType("nvarchar(max)");

            builder.HasIndex(x => x.JobName);

            builder.HasIndex(x => x.Status);

            builder.HasIndex(x => x.StartedAt);
        }
    }
}