using LinkUp.Application.Abstractions.Repositories;
using LinkUp.Application.Abstractions.Services;
using LinkUp.Domain.Base;
using System.Linq.Expressions;

namespace LinkUp.Application.Services;

// Implementación del servicio genérico que delega en el repositorio genérico.
public class GenericService<T> : IGenericService<T> where T : BaseEntity
{
    protected readonly IGenericRepository<T> _repository;

    public GenericService(IGenericRepository<T> repository)
    {
        _repository = repository;
    }

    public virtual Task<T?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public virtual Task<IEnumerable<T>> GetAllAsync() => _repository.GetAllAsync();
    public virtual Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => _repository.FindAsync(predicate);
    public virtual Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) => _repository.FirstOrDefaultAsync(predicate);
    public virtual Task AddAsync(T entity) => _repository.AddAsync(entity);
    public virtual Task UpdateAsync(T entity) => _repository.UpdateAsync(entity);
    public virtual Task DeleteAsync(T entity) => _repository.DeleteAsync(entity);
    public virtual Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => _repository.ExistsAsync(predicate);
}
