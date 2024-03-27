using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace AddOptimization.Data.Contracts
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        public int? CurrentUserId { get; set; }
        public Guid? CurrentBranchId { get; set; }
        Task<IQueryable<TEntity>> FromSqlAsync<TResult>(FormattableString sql);
        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null,
                                                       Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                       Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                       bool disableTracking = true);
        /// <summary>
        /// Gets the first or default entity based on a predicate, orderby delegate and include delegate. This method default no-tracking query.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="orderBy">A function to order elements.</param>
        /// <param name="include">A function to include navigation properties</param>
        /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <returns>An <see cref="IPagedList{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        /// <remarks>This method default no-tracking query.</remarks>
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null,
                                                       Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                       Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                       bool disableTracking = true,bool ignoreGlobalFilter = false);
        Task<IQueryable<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> predicate = null,
                                                  Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                  Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                  Expression<Func<TEntity, TEntity>> select = null,
                                                  bool disableTracking = true, bool ignoreGlobalFilter = false);
        Task<IQueryable<TResult>> QueryMappedAsync<TResult>(Expression<Func<TEntity, TResult>> select, Expression<Func<TEntity, bool>> predicate = null,
                                                 Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                 Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                 bool disableTracking = true, bool ignoreGlobalFilter = false);
        Task<TEntity> InsertAsync(TEntity entity);
        Task<List<TEntity>> BulkInsertAsync(List<TEntity> entities);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<List<TEntity>> BulkUpdateAsync(List<TEntity> entities);
        Task DeleteAsync(TEntity entity);
        Task BulkDeleteAsync(IList<TEntity> data);
        Task<bool> IsExist(Expression<Func<TEntity, bool>> predicate = null, bool ignoreGlobalFilter = false, params Expression<Func<TEntity, object>>[] includes);
        Task<int> MaxAsync(Expression<Func<TEntity, int>> select,Expression < Func<TEntity, bool>> predicate = null, bool ignoreGlobalFilter = false);
        Task<int> ExecuteSqlAsync(FormattableString sql);
        /// <summary>
        /// Trun on identity insert on for TEntity
        /// </summary>
        /// <returns></returns>
        Task<bool> TrunIdentityOn();
        /// <summary>
        /// Trun on identity insert off for TEntity
        /// </summary>
        /// <returns></returns>
        Task<bool> TrunIdentityOff();
    }
}
