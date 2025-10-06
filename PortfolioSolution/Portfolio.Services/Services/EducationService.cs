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
    public class EducationService
        : BaseCRUDService<EducationResponse, EducationSearchObject, Education, EducationInsertRequest, EducationUpdateRequest, Guid>,
          IEducationService
    {
        public EducationService(ApplicationDbContext context, IMapper mapper, ILogger<EducationService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<Education> ApplyFilter(IQueryable<Education> query, EducationSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.InstitutionName))
                query = query.Where(x => x.InstitutionName.Contains(search.InstitutionName));

            if (!string.IsNullOrWhiteSpace(search.Degree))
                query = query.Where(x => x.Degree.Contains(search.Degree));

            if (!string.IsNullOrWhiteSpace(search.EducationType))
                query = query.Where(x => x.EducationType == search.EducationType);

            if (search.IsCurrent.HasValue)
                query = query.Where(x => x.IsCurrent == search.IsCurrent.Value);

            return query;
        }

        protected override async Task BeforeInsertAsync(Education entity, EducationInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(Education entity, EducationUpdateRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }
    }
}
