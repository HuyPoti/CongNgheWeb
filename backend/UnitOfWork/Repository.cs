using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;

namespace backend.UnitOfWork;

public class Repository<T>(DbContext dbContext, IMapper mapper) : IRepository<T> where T : class
{

    public async Task<IEnumerable<TOut>> GetAllAsync<TOut>(CancellationToken cancellationToken)
    {
        return await dbContext.Set<T>()
        .ProjectTo<TOut>(mapper.ConfigurationProvider)
        .ToListAsync(cancellationToken);
    }
    public async Task<TOut?> GetByIdAsync<TOut>(Guid id, CancellationToken cancellationToken)
    {
        var keyProperty = dbContext.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.FirstOrDefault();
        var keyName = keyProperty?.Name ?? typeof(T).Name + "Id";
        return await dbContext.Set<T>().Where(e => EF.Property<Guid>(e, keyName) == id).ProjectTo<TOut>(mapper.ConfigurationProvider).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<TOut>> GetAsync<TOut>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await dbContext.Set<T>()
        .Where(predicate)
        .ProjectTo<TOut>(mapper.ConfigurationProvider)
        .ToListAsync(cancellationToken);
    }

    public T Insert(T entity){
        dbContext.Set<T>().Add(entity);
        return entity;
    }

    public T Update(T entity)
    {
        dbContext.Set<T>().Update(entity);
        return entity;
    }

    public void Delete(T entity)
    {
        dbContext.Set<T>().Remove(entity);
    }

    public async Task<T?> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
        if (entity is not null)
        {
            dbContext.Set<T>().Remove(entity);
        }
        return entity;
    }
    public IQueryable<T> Query()
    {
        return dbContext.Set<T>();
    }
}