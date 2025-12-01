using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Domain.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // Retrieve all entities asynchronously
        Task<IEnumerable<T>> GetAllAsync();

        // Retrieve entity by ID asynchronously
        Task<T?> GetByIdAsync(params object[] id);

        // Add a new entity asynchronously
        Task AddAsync(T entity);

        // Add a range of entities asynchronously
        Task AddRangeAsync(IEnumerable<T> entities);

        // Update an existing entity
        void Update(T entity);

        // Remove an entity
        void Remove(T entity);

        // Soft delete an entity (set IsDeleted = true)
        void SoftDelete(T entity);

        // Check if any entity matches the predicate
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

         
        // Counts entities matching a predicate
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
 
        // Finds all entities matching a predicate
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // Retrieve entities with filtering, ordering, and pagination
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string? includeProperties = null);  
    }
}