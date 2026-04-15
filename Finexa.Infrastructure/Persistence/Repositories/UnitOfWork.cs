using System.Collections.Concurrent;

namespace Finexa.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FinexaDbContext _context;
        private readonly ConcurrentDictionary<string, object> _repositories;

        public UnitOfWork(FinexaDbContext context)
        {
            _context = context;
            _repositories = new();
        }

        public IGenericRepository<TEntity, TKey> Repository<TEntity, TKey>()
            where TEntity : BaseEntity<TKey>
            where TKey : IEquatable<TKey>
        {
            return (IGenericRepository<TEntity, TKey>)_repositories.GetOrAdd(
                typeof(TEntity).FullName!,
                _ => new GenericRepository<TEntity, TKey>(_context)
            );
        }

        public Task<int> SaveChangesAsync()
            => _context.SaveChangesAsync();

        public ValueTask DisposeAsync()
            => _context.DisposeAsync();
    }
}