using System;
using identity_service.Data;
using identity_service.Models;
using identity_service.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace identity_service.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : EntityBase
{
    protected readonly AppDbContext _dbContext;

    public BaseRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> AddAsync(T entity)
    {
        entity.DateCreate = DateTime.UtcNow;
        await _dbContext.Set<T>().AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;    
    }

    public async Task<int> DeleteAsync(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
        return await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbContext.Set<T>().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    public async Task<int> UpdateAsync(T entity)
    {
        entity.DateUpdate = DateTime.UtcNow;
        _dbContext.Set<T>().Update(entity);
        return await _dbContext.SaveChangesAsync();
    }

    public IQueryable<T> Query()
    {
        // No ejecuta la consulta aún — solo devuelve el IQueryable
        return _dbContext.Set<T>().AsNoTracking();
    }
}