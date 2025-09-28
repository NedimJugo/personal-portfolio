using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.BaseInterfaces
{
    public interface ICRUDService<T, TSearch, TInsert, TUpdate, TId> : IService<T, TSearch, TId>
        where T : class
        where TSearch : BaseSearchObject
        where TInsert : class
        where TUpdate : class
        where TId : struct
    {
        Task<T> CreateAsync(TInsert request, CancellationToken cancellationToken = default);
        Task<T?> UpdateAsync(TId id, TUpdate request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteAsync(TId id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> CreateBulkAsync(IEnumerable<TInsert> requests, CancellationToken cancellationToken = default);
        Task<bool> DeleteBulkAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default);
    }
}
