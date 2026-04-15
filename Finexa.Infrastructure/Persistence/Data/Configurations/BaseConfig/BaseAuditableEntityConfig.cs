using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BaseAuditableEntityConfig<TEntity>
    : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseAuditableEntity<Guid>
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastModifiedAt)
            .IsRequired(false);
    }
}