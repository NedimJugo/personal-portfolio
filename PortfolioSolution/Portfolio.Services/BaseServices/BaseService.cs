using AutoMapper;
using EcoChallenge.Models.SearchObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portfolio.Services.BaseInterfaces;
using Portfolio.Services.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.BaseServices
{
    public abstract class BaseService<T, TSearch, TEntity> : IService<T, TSearch>
       where T : class
       where TSearch : BaseSearchObject
       where TEntity : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IMapper _mapper;
        protected readonly ILogger<BaseService<T, TSearch, TEntity>> _logger;

        protected BaseService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<BaseService<T, TSearch, TEntity>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual async Task<Models.Responses.PagedResult<T>> GetAsync(
            TSearch search,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = GetBaseQuery();
                query = ApplyFilter(query, search);
                query = ApplySorting(query, search);

                int? totalCount = null;
                if (search.IncludeTotalCount)
                {
                    totalCount = await query.CountAsync(cancellationToken);
                }

                if (!search.RetrieveAll)
                {
                    query = ApplyPaging(query, search);
                }

                var entities = await query.ToListAsync(cancellationToken);
                var items = entities.Select(MapToResponse).ToList();

                return new Models.Responses.PagedResult<T>
                {
                    Items = items,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting entities with search: {@Search}", search);
                throw;
            }
        }

        protected virtual IQueryable<TEntity> GetBaseQuery()
        {
            return _context.Set<TEntity>().AsQueryable();
        }

        protected virtual IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query, TSearch search)
        {
            return query;
        }

        protected virtual IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, TSearch search)
        {
            if (!string.IsNullOrWhiteSpace(search.SortBy) && search.IsValidSortField(search.SortBy))
            {
                var sortDirection = search.Desc ? "desc" : "asc";
                try
                {
                    return query.OrderBy($"{search.SortBy} {sortDirection}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Invalid sort field: {SortBy}, falling back to default", search.SortBy);
                    return query.OrderBy($"Id {sortDirection}");
                }
            }
            return query.OrderBy("Id");
        }

        protected virtual IQueryable<TEntity> ApplyPaging(IQueryable<TEntity> query, TSearch search)
        {
            if (search.Page.HasValue && search.Page.Value > 0)
            {
                query = query.Skip(search.Page.Value * (search.PageSize ?? 20));
            }

            if (search.PageSize.HasValue)
            {
                query = query.Take(search.PageSize.Value);
            }

            return query;
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await GetEntityByIdAsync(id, cancellationToken);
                return entity != null ? MapToResponse(entity) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting entity by id: {Id}", id);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var idList = ids.ToList();
                if (!idList.Any()) return Enumerable.Empty<T>();

                var entities = await _context.Set<TEntity>()
                    .Where(e => idList.Contains(EF.Property<int>(e, "Id")))
                    .ToListAsync(cancellationToken);

                return entities.Select(MapToResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting entities by ids: {@Ids}", ids);
                throw;
            }
        }

        public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Set<TEntity>()
                    .Where(e => EF.Property<int>(e, "Id") == id)
                    .AnyAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if entity exists: {Id}", id);
                throw;
            }
        }

        protected virtual async Task<TEntity?> GetEntityByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
        }

        protected virtual T MapToResponse(TEntity entity)
        {
            return _mapper.Map<T>(entity);
        }
    }
}
