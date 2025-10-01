using AutoMapper;
using EcoChallenge.Models.SearchObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portfolio.Services.BaseInterfaces;
using Portfolio.Services.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.BaseServices
{
    public abstract class BaseCRUDService<T, TSearch, TEntity, TInsert, TUpdate, TId>
        : BaseService<T, TSearch, TEntity, TId>, ICRUDService<T, TSearch, TInsert, TUpdate, TId>
        where T : class
        where TSearch : BaseSearchObject
        where TEntity : class, new()
        where TInsert : class
        where TUpdate : class
        where TId : struct
    {
        protected BaseCRUDService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<BaseService<T, TSearch, TEntity, TId>> logger)
            : base(context, mapper, logger)
        {
        }

        public virtual async Task<T> CreateAsync(TInsert request, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await ValidateInsertAsync(request, cancellationToken);

                var entity = new TEntity();
                MapInsertToEntity(entity, request);

                _context.Set<TEntity>().Add(entity);

                await BeforeInsertAsync(entity, request, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await AfterInsertAsync(entity, request, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                var idProperty = typeof(TEntity).GetProperty("Id");
                var idValue = idProperty?.GetValue(entity);
                _logger.LogInformation("Entity created successfully with id: {Id}", idValue);

                return MapToResponse(entity);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error occurred while creating entity: {@Request}", request);
                throw;
            }
        }

        public virtual async Task<T?> UpdateAsync(TId id, TUpdate request, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var entity = await GetEntityByIdAsync(id, cancellationToken);
                if (entity == null)
                {
                    _logger.LogWarning("Entity not found for update: {Id}", id);
                    return null;
                }

                await ValidateUpdateAsync(id, request, cancellationToken);
                await BeforeUpdateAsync(entity, request, cancellationToken);

                MapUpdateToEntity(entity, request);
                await _context.SaveChangesAsync(cancellationToken);

                await AfterUpdateAsync(entity, request, cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Entity updated successfully: {Id}", id);
                return MapToResponse(entity);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error occurred while updating entity {Id}: {@Request}", id, request);
                throw;
            }
        }

        public virtual async Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var entity = await GetEntityByIdAsync(id, cancellationToken);
                if (entity == null)
                {
                    _logger.LogWarning("Entity not found for deletion: {Id}", id);
                    return false;
                }

                await ValidateDeleteAsync(id, cancellationToken);
                await BeforeDeleteAsync(entity, cancellationToken);

                _context.Set<TEntity>().Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);

                await AfterDeleteAsync(entity, cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Entity deleted successfully: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error occurred while deleting entity: {Id}", id);
                throw;
            }
        }

        public virtual async Task<bool> SoftDeleteAsync(TId id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("Soft delete not implemented in base class");
        }

        public virtual async Task<IEnumerable<T>> CreateBulkAsync(
            IEnumerable<TInsert> requests,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var requestList = requests.ToList();
                var entities = new List<TEntity>();

                foreach (var request in requestList)
                {
                    await ValidateInsertAsync(request, cancellationToken);
                    var entity = new TEntity();
                    MapInsertToEntity(entity, request);
                    entities.Add(entity);
                }

                _context.Set<TEntity>().AddRange(entities);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Bulk created {Count} entities", entities.Count);
                return entities.Select(MapToResponse);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error occurred while bulk creating entities");
                throw;
            }
        }

        public virtual async Task<bool> DeleteBulkAsync(
            IEnumerable<TId> ids,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var idList = ids.ToList();
                var entities = await _context.Set<TEntity>()
                    .Where(e => idList.Contains(EF.Property<TId>(e, "Id")))
                    .ToListAsync(cancellationToken);

                if (!entities.Any())
                {
                    _logger.LogWarning("No entities found for bulk deletion: {@Ids}", ids);
                    return false;
                }

                _context.Set<TEntity>().RemoveRange(entities);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Bulk deleted {Count} entities", entities.Count);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error occurred while bulk deleting entities: {@Ids}", ids);
                throw;
            }
        }

        // Virtual methods for validation and hooks
        protected virtual async Task ValidateInsertAsync(TInsert request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task ValidateUpdateAsync(TId id, TUpdate request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task ValidateDeleteAsync(TId id, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task BeforeInsertAsync(TEntity entity, TInsert request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task AfterInsertAsync(TEntity entity, TInsert request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task BeforeUpdateAsync(TEntity entity, TUpdate request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task AfterUpdateAsync(TEntity entity, TUpdate request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task BeforeDeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task AfterDeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        protected virtual TEntity MapInsertToEntity(TEntity entity, TInsert request)
        {
            return _mapper.Map(request, entity);
        }

        protected virtual void MapUpdateToEntity(TEntity entity, TUpdate request)
        {
            _mapper.Map(request, entity);
        }
    }
}
