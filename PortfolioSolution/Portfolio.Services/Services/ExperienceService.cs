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
    public class ExperienceService
        : BaseCRUDService<ExperienceResponse, ExperienceSearchObject, Experience, ExperienceInsertRequest, ExperienceUpdateRequest, Guid>,
          IExperienceService
    {
        public ExperienceService(ApplicationDbContext context, IMapper mapper, ILogger<ExperienceService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<Experience> ApplyFilter(IQueryable<Experience> query, ExperienceSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.CompanyName))
                query = query.Where(x => x.CompanyName.Contains(search.CompanyName));

            if (!string.IsNullOrWhiteSpace(search.Position))
                query = query.Where(x => x.Position.Contains(search.Position));

            if (search.EmploymentType.HasValue)
                query = query.Where(x => x.EmploymentType == search.EmploymentType.Value);

            if (search.IsCurrent.HasValue)
                query = query.Where(x => x.IsCurrent == search.IsCurrent.Value);

            return query;
        }

        protected override async Task BeforeInsertAsync(Experience entity, ExperienceInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(Experience entity, ExperienceUpdateRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }
    }
}
