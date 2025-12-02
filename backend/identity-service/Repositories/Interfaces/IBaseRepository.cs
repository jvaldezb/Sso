using System;
using identity_service.Models;

namespace identity_service.Repositories.Interfaces;

public interface IBaseRepository<T> where T : EntityBase
{
    Task<IReadOnlyList<T>> GetAllAsync();    
    Task<T?> GetByIdAsync(Guid id);
    Task<T> AddAsync(T entity);
    Task<int> UpdateAsync(T entity);
    Task<int> DeleteAsync(T entity);
    IQueryable<T> Query();
}
