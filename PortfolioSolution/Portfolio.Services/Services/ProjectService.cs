using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
    public class ProjectService
        : BaseCRUDService<ProjectResponse, ProjectSearchObject, Project, ProjectInsertRequest, ProjectUpdateRequest, Guid>,
          IProjectService
    {
        public ProjectService(ApplicationDbContext context, IMapper mapper, ILogger<ProjectService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<Project> ApplyFilter(IQueryable<Project> query, ProjectSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.Title))
                query = query.Where(x => x.Title.Contains(search.Title));

            if (!string.IsNullOrWhiteSpace(search.Status))
                query = query.Where(x => x.Status == search.Status);

            if (!string.IsNullOrWhiteSpace(search.ProjectType))
                query = query.Where(x => x.ProjectType == search.ProjectType);

            if (search.IsFeatured.HasValue)
                query = query.Where(x => x.IsFeatured == search.IsFeatured);

            if (search.IsPublished.HasValue)
                query = query.Where(x => x.IsPublished == search.IsPublished);

            return query;
        }

        protected override IQueryable<Project> ApplyIncludes(IQueryable<Project> query)
        {
            return query
                .Include(p => p.Images)
                .Include(p => p.ProjectTags)
                .Include(p => p.ProjectTechs);
        }

        protected override async Task BeforeInsertAsync(Project entity, ProjectInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            entity.ViewCount = 0;

            entity.ProjectTags = request.TagIds.Select(tagId => new ProjectTag { TagId = tagId, ProjectId = entity.Id }).ToList();
            entity.ProjectTechs = request.TechIds.Select(techId => new ProjectTech { TechId = techId, ProjectId = entity.Id }).ToList();
            entity.Images = request.Images.Select(img => new ProjectImage
            {
                MediaId = img.MediaId,
                Caption = img.Caption,
                Order = img.Order,
                IsHero = img.IsHero
            }).ToList();

            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(Project entity, ProjectUpdateRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            if (request.TagIds != null)
            {
                entity.ProjectTags.Clear();
                foreach (var tagId in request.TagIds)
                    entity.ProjectTags.Add(new ProjectTag { TagId = tagId, ProjectId = entity.Id });
            }

            if (request.TechIds != null)
            {
                entity.ProjectTechs.Clear();
                foreach (var techId in request.TechIds)
                    entity.ProjectTechs.Add(new ProjectTech { TechId = techId, ProjectId = entity.Id });
            }

            if (request.Images != null)
            {
                entity.Images.Clear();
                foreach (var img in request.Images)
                {
                    entity.Images.Add(new ProjectImage
                    {
                        MediaId = img.MediaId,
                        Caption = img.Caption,
                        Order = img.Order ?? 0,
                        IsHero = img.IsHero ?? false,
                        ProjectId = entity.Id
                    });
                }
            }

            await Task.CompletedTask;
        }
    }
}
