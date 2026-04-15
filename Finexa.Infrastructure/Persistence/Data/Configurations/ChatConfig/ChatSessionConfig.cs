using Finexa.Domain.Entities.Ai.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finexa.Infrastructure.Persistence.Data.Configurations.ChatConfig
{
    
    public class ChatSessionConfig : BaseAuditableEntityConfig<ChatSession>
    {
        public override void Configure(EntityTypeBuilder<ChatSession> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Title)
                .HasMaxLength(200);

            builder.Property(x => x.Summary)
                .HasMaxLength(2000);

            builder.Property(x => x.LastActivityAt)
                .IsRequired();


            builder.HasMany(x => x.Messages)
                .WithOne(x => x.Session)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.AppUserId);
        }
    }
}
