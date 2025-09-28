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
    public class TestimonialService
        : BaseCRUDService<TestimonialResponse, TestimonialSearchObject, Testimonial, TestimonialInsertRequest, TestimonialUpdateRequest, Guid>,
          ITestimonialService
    {
        public TestimonialService(ApplicationDbContext context, IMapper mapper, ILogger<TestimonialService> logger)
            : base(context, mapper, logger) { }

        protected override IQueryable<Testimonial> ApplyFilter(IQueryable<Testimonial> query, TestimonialSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.ClientName))
                query = query.Where(x => x.ClientName.Contains(search.ClientName));

            if (search.IsApproved.HasValue)
                query = query.Where(x => x.IsApproved == search.IsApproved.Value);

            if (search.IsFeatured.HasValue)
                query = query.Where(x => x.IsFeatured == search.IsFeatured.Value);

            if (search.ProjectId.HasValue)
                query = query.Where(x => x.ProjectId == search.ProjectId.Value);

            return query;
        }

        protected override async Task BeforeInsertAsync(Testimonial entity, TestimonialInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            entity.IsDeleted = false;
            await Task.CompletedTask;
        }

        protected override async Task BeforeUpdateAsync(Testimonial entity, TestimonialUpdateRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await Task.CompletedTask;
        }
    }
}
