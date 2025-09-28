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
    public class SkillService
        : BaseCRUDService<SkillResponse, SkillSearchObject, Skill, SkillInsertRequest, SkillUpdateRequest, Guid>,
          ISkillService
    {
        public SkillService(ApplicationDbContext context, IMapper mapper, ILogger<SkillService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<Skill> ApplyFilter(IQueryable<Skill> query, SkillSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.Name))
                query = query.Where(x => x.Name.Contains(search.Name));

            if (!string.IsNullOrWhiteSpace(search.Category))
                query = query.Where(x => x.Category == search.Category);

            if (search.IsFeatured.HasValue)
                query = query.Where(x => x.IsFeatured == search.IsFeatured.Value);

            if (search.MinProficiency.HasValue)
                query = query.Where(x => x.ProficiencyLevel >= search.MinProficiency.Value);

            if (search.MaxProficiency.HasValue)
                query = query.Where(x => x.ProficiencyLevel <= search.MaxProficiency.Value);

            return query;
        }

        protected override async Task BeforeInsertAsync(Skill entity, SkillInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(Skill entity, SkillUpdateRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }
    }
}
