using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finexa.Infrastructure.Persistence.Data.Configurations.UserBalanceConfig
{
    public class UserBalanceConfig : BaseAuditableEntityConfig<UserBalance>
    {
        public override void Configure(EntityTypeBuilder<UserBalance> builder)
        {
            base.Configure(builder);

            builder.Property(b => b.TotalIncome)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.TotalExpense)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.TotalBalance)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasOne(b => b.AppUser)
                .WithOne(u => u.UserBalance)
                .HasForeignKey<UserBalance>(b => b.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(b => b.AppUserId)
                .IsUnique();
        }
    }
}