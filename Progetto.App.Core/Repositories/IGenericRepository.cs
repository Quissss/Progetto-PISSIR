using System.Linq.Expressions;

namespace Progetto.App.Core.Repositories;

/// <summary>
/// Generic repository interface
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IGenericRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task DeleteAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<int> SaveAsync();
}