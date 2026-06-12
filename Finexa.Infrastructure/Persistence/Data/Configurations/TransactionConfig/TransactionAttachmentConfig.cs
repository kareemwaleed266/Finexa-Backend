using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finexa.Infrastructure.Persistence.Data.Configurations.TransactionConfig
{
    public class TransactionAttachmentConfig : BaseAuditableEntityConfig<TransactionAttachment>
    {
        public override void Configure(EntityTypeBuilder<TransactionAttachment> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Url)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.PublicId)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.SizeInBytes)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.HasOne(x => x.Transaction)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.TransactionId);
        }
    }
}
