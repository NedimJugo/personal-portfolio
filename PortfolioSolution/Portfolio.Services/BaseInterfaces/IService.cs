using EcoChallenge.Models.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.BaseInterfaces
{
    public interface IService<T, TSearch>
        where T : class
        where TSearch : BaseSearchObject
    {
        Task<Models.Responses.PagedResult<T>> GetAsync(TSearch search, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    }
}
