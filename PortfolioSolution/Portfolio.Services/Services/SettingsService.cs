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
    public class SettingsService
        : BaseCRUDService<SettingsResponse, SettingsSearchObject, Settings, SettingsInsertRequest, SettingsUpdateRequest, Guid>,
          ISettingsService
    {
        public SettingsService(ApplicationDbContext context, IMapper mapper, ILogger<SettingsService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<Settings> ApplyFilter(IQueryable<Settings> query, SettingsSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.Key))
                query = query.Where(x => x.Key.Contains(search.Key));

            return query;
        }

        protected override async Task BeforeInsertAsync(Settings entity, SettingsInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(Settings entity, SettingsUpdateRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }
    }
}
