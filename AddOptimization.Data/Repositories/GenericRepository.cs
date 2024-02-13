using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using AddOptimization.Data.Common;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Utilities.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace AddOptimization.Data.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly AddOptimizationContext _context;
        private readonly ILogger<GenericRepository<TEntity>> _logger;
        private DbSet<TEntity> entities;
        public int? CurrentUserId { get; set; }
        public Guid? CurrentBranchId { get; set; }

        public GenericRepository(AddOptimizationContext context, ILogger<GenericRepository<TEntity>> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            entities = context.Set<TEntity>();
            CurrentUserId = httpContextAccessor.HttpContext.GetCurrentUserId();
            CurrentBranchId = httpContextAccessor.HttpContext.GetBranchId();
        }
        private string GetTableName()
        {
            var entityType = _context.Model.FindEntityType(typeof(TEntity));
            if (entityType != null)
            {
                return entityType.GetTableName();
            }
            throw new ArgumentException($"The entity type {typeof(TEntity)} is not part of the model.");
        }

        public async Task<bool> TrunIdentityOn()
        {
            try
            {
                var tableName = GetTableName();
                await ExecuteSqlAsync($"set Identity_Insert {tableName} on");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<bool> TrunIdentityOff()
        {
            try
            {
                var tableName = GetTableName();
                await ExecuteSqlAsync($"set Identity_Insert {tableName} off");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<IQueryable<TEntity>> FromSqlAsync<TResult>(FormattableString sql)
        {
            try
            {
                return await Task.FromResult(entities.FromSql(sql));
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<int> ExecuteSqlAsync(FormattableString sql)
        {
            try
            {
                return await _context.Database.ExecuteSqlAsync(sql);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }



        public async Task DeleteAsync(TEntity entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException();
                }
                entities.Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<IQueryable<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> predicate = null,
                                                 Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                 Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                 Expression<Func<TEntity, TEntity>> select = null,
                                                 bool disableTracking = true, bool ignoreGlobalFilter = false)
        {
            try
            {
                IQueryable<TEntity> query = entities;

                if (ignoreGlobalFilter)
                {
                    query = entities.IgnoreQueryFilters();
                }

                if (disableTracking)
                {
                    query = query.AsNoTracking();
                }

                if (include != null)
                {
                    query = include(query);
                }

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }
                if (orderBy != null)
                {
                    return await Task.FromResult(select != null ? orderBy(query).Select(select) : orderBy(query));
                }
                else
                {
                    return await Task.FromResult(select != null ? query.Select(select) : query);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        public async Task<IQueryable<TResult>> QueryMappedAsync<TResult>(Expression<Func<TEntity, TResult>> select, Expression<Func<TEntity, bool>> predicate = null,
                                               Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                               Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                               bool disableTracking = true, bool ignoreGlobalFilter = false)
        {
            try
            {
                IQueryable<TEntity> query = entities;
                if (disableTracking)
                {
                    query = query.AsNoTracking();
                }
                if (ignoreGlobalFilter)
                {
                    query = entities.IgnoreQueryFilters();
                }

                if (include != null)
                {
                    query = include(query);
                }

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                if (orderBy != null)
                {

                    return await Task.FromResult(orderBy(query).Select(select)); ;
                }
                else
                {
                    return await Task.FromResult(query.Select(select)); ;
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<bool> IsExist(Expression<Func<TEntity, bool>> predicate = null, params Expression<Func<TEntity, object>>[] includes)
        {
            try
            {
                var query = entities.AsQueryable();
                if (includes != null)
                {
                    query = includes.Aggregate(query,
                              (current, include) => current.Include(include));
                }
                return await Task.FromResult(query.Any(predicate));
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null,
                                                Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                bool disableTracking = true)
        {
            try
            {
                IQueryable<TEntity> query = entities;
                if (disableTracking)
                {
                    query = query.AsNoTracking();
                }

                if (include != null)
                {
                    query = include(query);
                }

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                if (orderBy != null)
                {
                    return await orderBy(query).SingleOrDefaultAsync();
                }
                else
                {
                    return await query.SingleOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null,
                                                  Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                                                  Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
                                                  bool disableTracking = true, bool ignoreGlobalFilter = false)
        {
            try
            {
                IQueryable<TEntity> query = entities;
                if (ignoreGlobalFilter)
                {
                    query = entities.IgnoreQueryFilters();
                }

                if (disableTracking)
                {
                    query = query.AsNoTracking();
                }

                if (include != null)
                {
                    query = include(query);
                }

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                if (orderBy != null)
                {
                    return await orderBy(query).FirstOrDefaultAsync();
                }
                else
                {
                    return await query.FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task BulkDeleteAsync(IList<TEntity> data)
        {
            try
            {
                if (data == null)
                {
                    throw new ArgumentNullException();
                }
                entities.RemoveRange(data);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public bool HasProperty<TProp>(string propertyName)
        {
            Type entityType = typeof(TEntity);
            PropertyInfo property = entityType.GetProperty(propertyName);
            return property != null && property.PropertyType == typeof(TProp);
        }
        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException();
                }
                var time = DateTime.UtcNow;
                if (HasProperty<DateTime?>("CreatedAt") && entity.GetType().GetProperty("CreatedAt").GetValue(entity) == null)
                {
                    entity.GetType().GetProperty("CreatedAt").SetValue(entity, time);
                }
                if (HasProperty<int?>("CreatedByUserId") && entity.GetType().GetProperty("CreatedByUserId").GetValue(entity) == null)
                {
                    entity.GetType().GetProperty("CreatedByUserId").SetValue(entity, CurrentUserId);
                }
                if (HasProperty<Guid?>("BranchId") && entity.GetType().GetProperty("BranchId").GetValue(entity) == null)
                {
                    entity.GetType().GetProperty("BranchId").SetValue(entity, CurrentBranchId);
                }
                entities.Add(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }

        }

        public async Task<List<TEntity>> BulkInsertAsync(List<TEntity> data)
        {


            try
            {
                var time = DateTime.UtcNow;
                foreach (var entity in data)
                {
                    if (HasProperty<DateTime?>("CreatedAt") && entity.GetType().GetProperty("CreatedAt").GetValue(entity) == null)
                    {
                        entity.GetType().GetProperty("CreatedAt").SetValue(entity, time);
                    }
                    if (HasProperty<int?>("CreatedByUserId") && entity.GetType().GetProperty("CreatedByUserId").GetValue(entity) == null)
                    {
                        entity.GetType().GetProperty("CreatedByUserId").SetValue(entity, CurrentUserId);
                    }
                    if (HasProperty<Guid?>("BranchId") && entity.GetType().GetProperty("BranchId").GetValue(entity) == null)
                    {
                        entity.GetType().GetProperty("BranchId").SetValue(entity, CurrentBranchId);
                    }
                }
                entities.AddRange(data);
                await _context.SaveChangesAsync();
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException();
                }
                var time = DateTime.UtcNow;
                if (HasProperty<DateTime?>("UpdatedAt"))
                {
                    entity.GetType().GetProperty("UpdatedAt").SetValue(entity, time);
                }
                if (HasProperty<int?>("UpdatedByUserId"))
                {
                    entity.GetType().GetProperty("UpdatedByUserId").SetValue(entity, CurrentUserId);
                }
                entities.Update(entity);
                await _context.SaveChangesAsync();
                if (entity is BaseEntityCreatedDateOnly<int>)
                {
                    _context.Entry(entity).Property(e => (e as BaseEntityCreatedDateOnly<int>).CreatedAt).IsModified = false;
                }
                if (entity is BaseEntityCreatedDateOnly<Guid>)
                {
                    _context.Entry(entity).Property(e => (e as BaseEntityCreatedDateOnly<Guid>).CreatedAt).IsModified = false;
                }
                if (entity is BaseEntityNewCreatedOnlyProps<int>)
                {
                    _context.Entry(entity).Property(e => (e as BaseEntityNewCreatedOnlyProps<int>).CreatedByUserId).IsModified = false;
                }
                if (entity is BaseEntityNewCreatedOnlyProps<Guid>)
                {
                    _context.Entry(entity).Property(e => (e as BaseEntityNewCreatedOnlyProps<Guid>).CreatedByUserId).IsModified = false;
                }

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<List<TEntity>> BulkUpdateAsync(List<TEntity> data)
        {
            try
            {
                var time = DateTime.UtcNow;
                foreach (var entity in data)
                {
                    if (HasProperty<DateTime?>("UpdatedAt"))
                    {
                        entity.GetType().GetProperty("UpdatedAt").SetValue(entity, time);
                    }
                    if (HasProperty<int?>("UpdatedByUserId"))
                    {
                        entity.GetType().GetProperty("UpdatedByUserId").SetValue(entity, CurrentUserId);
                    }
                }
                entities.UpdateRange(data);
                foreach (var entity in data)
                {
                    if (entity is BaseEntityCreatedDateOnly<int>)
                    {
                        _context.Entry(entity).Property(e => (e as BaseEntityCreatedDateOnly<int>).CreatedAt).IsModified = false;
                    }
                    if (entity is BaseEntityCreatedDateOnly<Guid>)
                    {
                        _context.Entry(entity).Property(e => (e as BaseEntityCreatedDateOnly<Guid>).CreatedAt).IsModified = false;
                    }
                    if (entity is BaseEntityNewCreatedOnlyProps<int>)
                    {
                        _context.Entry(entity).Property(e => (e as BaseEntityNewCreatedOnlyProps<int>).CreatedByUserId).IsModified = false;
                    }
                    if (entity is BaseEntityNewCreatedOnlyProps<Guid>)
                    {
                        _context.Entry(entity).Property(e => (e as BaseEntityNewCreatedOnlyProps<Guid>).CreatedByUserId).IsModified = false;
                    }

                }
                await _context.SaveChangesAsync();
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        public async Task<int> MaxAsync(Expression<Func<TEntity, int>> select, Expression<Func<TEntity, bool>> predicate = null, bool ignoreGlobalFilter = false)
        {
            try
            {
                IQueryable<TEntity> query = entities;
                if (ignoreGlobalFilter)
                {
                    query = entities.IgnoreQueryFilters();
                }
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }
                return query.Any() ? await query.MaxAsync(select) : 0;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

    }
}
