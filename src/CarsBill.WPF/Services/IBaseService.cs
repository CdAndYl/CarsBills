using System.Linq.Expressions;

namespace CarsBill.WPF.Services;

/// <summary>
/// Generic CRUD service interface
/// </summary>
public interface IBaseService<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(object id);
    Task<List<T>> QueryAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(object id);
}
