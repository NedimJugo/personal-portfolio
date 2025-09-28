using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.BaseInterfaces
{
    public interface IService<T, TSearch, TId>
        where T : class
        where TSearch : BaseSearchObject
        where TId : struct
    {
        Task<Models.Responses.PagedResult<T>> GetAsync(TSearch search, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
    }
}
