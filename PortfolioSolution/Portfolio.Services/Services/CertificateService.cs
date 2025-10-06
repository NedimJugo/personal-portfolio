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
    public class CertificateService
        : BaseCRUDService<CertificateResponse, CertificateSearchObject, Certificate, CertificateInsertRequest, CertificateUpdateRequest, Guid>,
          ICertificateService
    {
        public CertificateService(ApplicationDbContext context, IMapper mapper, ILogger<CertificateService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<Certificate> ApplyFilter(IQueryable<Certificate> query, CertificateSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.Name))
                query = query.Where(x => x.Name.Contains(search.Name));

            if (!string.IsNullOrWhiteSpace(search.IssuingOrganization))
                query = query.Where(x => x.IssuingOrganization.Contains(search.IssuingOrganization));

            if (!string.IsNullOrWhiteSpace(search.CertificateType))
                query = query.Where(x => x.CertificateType == search.CertificateType);

            if (search.IsActive.HasValue)
                query = query.Where(x => x.IsActive == search.IsActive.Value);

            if (search.IsPublished.HasValue)
                query = query.Where(x => x.IsPublished == search.IsPublished.Value);

            return query;
        }

        protected override async Task BeforeInsertAsync(Certificate entity, CertificateInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(Certificate entity, CertificateUpdateRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }
    }
}
