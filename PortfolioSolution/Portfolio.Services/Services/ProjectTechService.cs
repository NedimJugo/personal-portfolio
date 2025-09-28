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
    public class ProjectTechService
        : BaseCRUDService<ProjectTechResponse, ProjectTechSearchObject, ProjectTech, ProjectTechInsertRequest, ProjectTechUpdateRequest, Guid>,
          IProjectTechService
    {
        public ProjectTechService(ApplicationDbContext context, IMapper mapper, ILogger<ProjectTechService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<ProjectTech> ApplyFilter(IQueryable<ProjectTech> query, ProjectTechSearchObject? search = null)
        {
            if (search == null) return query;

            if (search.ProjectId.HasValue)
                query = query.Where(x => x.ProjectId == search.ProjectId.Value);

            if (search.TechId.HasValue)
                query = query.Where(x => x.TechId == search.TechId.Value);

            return query;
        }
    }
}
