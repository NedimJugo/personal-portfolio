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
    public class ProjectTagService
       : BaseCRUDService<ProjectTagResponse, ProjectTagSearchObject, ProjectTag, ProjectTagInsertRequest, ProjectTagUpdateRequest, Guid>,
         IProjectTagService
    {
        public ProjectTagService(ApplicationDbContext context, IMapper mapper, ILogger<ProjectTagService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<ProjectTag> ApplyFilter(IQueryable<ProjectTag> query, ProjectTagSearchObject? search = null)
        {
            if (search == null) return query;

            if (search.ProjectId.HasValue)
                query = query.Where(x => x.ProjectId == search.ProjectId.Value);

            if (search.TagId.HasValue)
                query = query.Where(x => x.TagId == search.TagId.Value);

            return query;
        }
    }
}
