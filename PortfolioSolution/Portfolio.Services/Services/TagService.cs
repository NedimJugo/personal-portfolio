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
    public class TagService
        : BaseCRUDService<TagResponse, TagSearchObject, Tag, TagInsertRequest, TagUpdateRequest, Guid>,
          ITagService
    {
        public TagService(ApplicationDbContext context, IMapper mapper, ILogger<TagService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<Tag> ApplyFilter(IQueryable<Tag> query, TagSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.Name))
                query = query.Where(x => x.Name.Contains(search.Name));

            if (!string.IsNullOrWhiteSpace(search.Slug))
                query = query.Where(x => x.Slug.Contains(search.Slug));

            return query;
        }

        protected override async Task BeforeInsertAsync(Tag entity, TagInsertRequest request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(Tag entity, TagUpdateRequest request, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }
    }
}
