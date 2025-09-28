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
    public class ProjectImageService
        : BaseCRUDService<ProjectImageResponse, ProjectImageSearchObject, ProjectImage, ProjectImageInsertRequest, ProjectImageUpdateRequest, Guid>,
          IProjectImageService
    {
        public ProjectImageService(ApplicationDbContext context, IMapper mapper, ILogger<ProjectImageService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<ProjectImage> ApplyFilter(IQueryable<ProjectImage> query, ProjectImageSearchObject? search = null)
        {
            if (search == null) return query;

            if (search.ProjectId.HasValue)
                query = query.Where(x => x.ProjectId == search.ProjectId.Value);

            if (search.MediaId.HasValue)
                query = query.Where(x => x.MediaId == search.MediaId.Value);

            if (search.IsHero.HasValue)
                query = query.Where(x => x.IsHero == search.IsHero.Value);

            return query;
        }
    }
}
