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
    public class EmailTemplateService
        : BaseCRUDService<EmailTemplateResponse, EmailTemplateSearchObject, EmailTemplate, EmailTemplateInsertRequest, EmailTemplateUpdateRequest, Guid>,
          IEmailTemplateService
    {
        public EmailTemplateService(ApplicationDbContext context, IMapper mapper, ILogger<EmailTemplateService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<EmailTemplate> ApplyFilter(IQueryable<EmailTemplate> query, EmailTemplateSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.Name))
                query = query.Where(x => x.Name.Contains(search.Name));

            if (search.IsActive.HasValue)
                query = query.Where(x => x.IsActive == search.IsActive.Value);

            return query;
        }

        protected override async Task BeforeInsertAsync(EmailTemplate entity, EmailTemplateInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(EmailTemplate entity, EmailTemplateUpdateRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }
    }
}
