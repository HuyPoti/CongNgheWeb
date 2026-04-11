using System.Linq.Expressions;

namespace backend.UnitOfWork;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<TOut>> GetAllAsync<TOut>(CancellationToken cancellationToken);
    Task<TOut?> GetByIdAsync<TOut>(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<TOut>> GetAsync<TOut>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);

    T Insert(T entity);
    T Update(T entity);
    Task<T?> DeleteAsync(Guid id, CancellationToken cancellationToken);
    IQueryable<T> Query();
}