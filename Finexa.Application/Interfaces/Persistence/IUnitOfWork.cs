using Finexa.Domain.Common.Entities;

public interface IUnitOfWork : IAsyncDisposable
{
    IGenericRepository<TEntity, TKey> Repository<TEntity, TKey>()
        where TEntity : BaseEntity<TKey>
        where TKey : IEquatable<TKey>;

    Task<int> SaveChangesAsync();
}