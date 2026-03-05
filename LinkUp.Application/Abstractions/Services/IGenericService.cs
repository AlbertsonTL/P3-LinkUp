using LinkUp.Domain.Base;
using System.Linq.Expressions;

namespace LinkUp.Application.Abstractions.Services;

// Servicio genérico para operaciones CRUD sobre entidades
// Complementa el patrón de repositorio genérico
public interface IGenericService<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}
