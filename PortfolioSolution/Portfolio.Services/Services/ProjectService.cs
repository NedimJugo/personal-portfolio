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

            await Task.CompletedTask;
        }

        protected override async Task AfterUpdateAsync(Project entity, ProjectUpdateRequest request, CancellationToken cancellationToken = default)
        {
            // Handle Tags relationship - REMOVE OLD, THEN ADD NEW
            if (request.TagIds != null)
            {
                // Remove existing tags for this project
                var existingTags = await _context.ProjectTags
                    .Where(pt => pt.ProjectId == entity.Id)
                    .ToListAsync(cancellationToken);

                if (existingTags.Any())
                {
                    _context.ProjectTags.RemoveRange(existingTags);
                }

                // Add new tags
                if (request.TagIds.Any())
                {
                    var newTags = request.TagIds.Select(tagId => new ProjectTag
                    {
                        ProjectId = entity.Id,
                        TagId = tagId
                    }).ToList();

                    await _context.ProjectTags.AddRangeAsync(newTags, cancellationToken);
                }
            }

            // Handle Techs relationship - REMOVE OLD, THEN ADD NEW
            if (request.TechIds != null)
            {
                // Remove existing techs for this project
                var existingTechs = await _context.ProjectTechs
                    .Where(pt => pt.ProjectId == entity.Id)
                    .ToListAsync(cancellationToken);

                if (existingTechs.Any())
                {
                    _context.ProjectTechs.RemoveRange(existingTechs);
                }

                // Add new techs
                if (request.TechIds.Any())
                {
                    var newTechs = request.TechIds.Select(techId => new ProjectTech
                    {
                        ProjectId = entity.Id,
                        TechId = techId
                    }).ToList();

                    await _context.ProjectTechs.AddRangeAsync(newTechs, cancellationToken);
                }
            }

            // Handle Images relationship - REMOVE OLD, THEN ADD NEW
            if (request.Images != null)
            {
                // Remove existing images for this project
                var existingImages = await _context.ProjectImages
                    .Where(pi => pi.ProjectId == entity.Id)
                    .ToListAsync(cancellationToken);

                if (existingImages.Any())
                {
                    _context.ProjectImages.RemoveRange(existingImages);
                }

                // Add new images
                if (request.Images.Any())
                {
                    var newImages = request.Images.Select(img => new ProjectImage
                    {
                        ProjectId = entity.Id,
                        MediaId = img.MediaId,
                        Caption = img.Caption ?? string.Empty,
                        Order = img.Order ?? 0,
                        IsHero = img.IsHero ?? false
                    }).ToList();

                    await _context.ProjectImages.AddRangeAsync(newImages, cancellationToken);
                }
            }

            // Save all changes
            await _context.SaveChangesAsync(cancellationToken);

            await base.AfterUpdateAsync(entity, request, cancellationToken);
        }

        protected override async Task<Project?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Project>()
                .Include(p => p.Images)
                .Include(p => p.ProjectTags)
                .Include(p => p.ProjectTechs)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
        protected override ProjectResponse MapToResponse(Project entity)
        {
            var response = base.MapToResponse(entity);

            // Ensure tagIds and techIds are populated
            response.TagIds = entity.ProjectTags?.Select(pt => pt.TagId).ToList() ?? new List<Guid>();
            response.TechIds = entity.ProjectTechs?.Select(pt => pt.TechId).ToList() ?? new List<Guid>();

            return response;
        }
    }
}
