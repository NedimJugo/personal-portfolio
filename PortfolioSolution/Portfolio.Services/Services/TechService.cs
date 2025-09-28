using AutoMapper;
using Microsoft.Extensions.Logging;
using Portfolio.Models.Requests.InsertRequests;
using Portfolio.Models.Requests.UpdateRequests;
using Portfolio.Models.Responses;
using Portfolio.Models.SearchObjects;
using Portfolio.Services.BaseServices;
using Portfolio.Services.Database.Entities;
using Portfolio.Services.Database;
using Portfolio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Services
{
    public class TechService
        : BaseCRUDService<TechResponse, TechSearchObject, Tech, TechInsertRequest, TechUpdateRequest, Guid>,
          ITechService
    {
        public TechService(ApplicationDbContext context, IMapper mapper, ILogger<TechService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<Tech> ApplyFilter(IQueryable<Tech> query, TechSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.Name))
                query = query.Where(x => x.Name.Contains(search.Name));

            if (!string.IsNullOrWhiteSpace(search.Slug))
                query = query.Where(x => x.Slug.Contains(search.Slug));

            if (!string.IsNullOrWhiteSpace(search.Category))
                query = query.Where(x => x.Category == search.Category);

            return query;
        }

        protected override async Task BeforeInsertAsync(Tech entity, TechInsertRequest request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(Tech entity, TechUpdateRequest request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }
    }

}
