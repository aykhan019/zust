using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Zust.Core.Abstraction;

namespace Zust.Core.Concrete.EntityFramework
{
    /// <summary>
    /// Represents a base class for Entity Framework-based implementation of IEntityRepository.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <typeparam name="TContext">The type of DbContext.</typeparam>
    public class EfEntityRepositoryBase<TEntity, TContext>
        : IEntityRepository<TEntity>
        where TEntity : class, IEntity, new()
        where TContext : DbContext, new()
    {
        /// <summary>
        /// Asynchronously adds a new entity to the DbContext and saves the changes.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddAsync(TEntity entity)
        {
            using var context = new TContext();

            var addedEntity = context.Entry(entity);

            addedEntity.State = EntityState.Added;

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Asynchronously deletes an existing entity from the DbContext and saves the changes.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeleteAsync(TEntity entity)
        {
            using var context = new TContext();

            var deletedEntity = context.Entry(entity);

            deletedEntity.State = EntityState.Deleted;

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Asynchronously updates an existing entity in the DbContext and saves the changes.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateAsync(TEntity entity)
        {
            using var context = new TContext();

            var updatedEntity = context.Entry(entity);

            updatedEntity.State = EntityState.Modified;

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a single entity from the DbContext based on the provided filter expression.
        /// </summary>
        /// <param name="filter">The filter expression to apply for entity retrieval.</param>
        /// <returns>A task that represents the asynchronous operation with the retrieved entity, or null if not found.</returns>
        public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter)
        {
            using var context = new TContext();

            // Read-only fetch: skip change-tracking for speed and lower memory.
            return await context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(filter);
        }

        /// <summary>
        /// Asynchronously retrieves all entities from the DbContext based on the optional filter expression.
        /// </summary>
        /// <param name="filter">The optional filter expression to apply for entity retrieval.</param>
        /// <returns>A task that represents the asynchronous operation with the collection of retrieved entities.</returns>
        public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null)
        {
            using var context = new TContext();

            // Read-only fetch: skip change-tracking for speed and lower memory.
            var items = context.Set<TEntity>().AsNoTracking();

            return filter == null ? await items.ToListAsync() : await items.Where(filter).ToListAsync();
        }
    }
}
