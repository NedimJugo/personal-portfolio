using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
    public class ApplicationUserService
        : BaseCRUDService<ApplicationUserResponse, ApplicationUserSearchObject, ApplicationUser, ApplicationUserInsertRequest, ApplicationUserUpdateRequest, int>,
          IApplicationUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationUserService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<ApplicationUserService> logger,
            UserManager<ApplicationUser> userManager
        ) : base(context, mapper, logger)
        {
            _userManager = userManager;
        }

        protected override IQueryable<ApplicationUser> ApplyFilter(IQueryable<ApplicationUser> query, ApplicationUserSearchObject? search = null)
        {
            if (search == null) return query;

            if (!string.IsNullOrWhiteSpace(search.UserName))
                query = query.Where(x => x.UserName.Contains(search.UserName));

            if (!string.IsNullOrWhiteSpace(search.Email))
                query = query.Where(x => x.Email.Contains(search.Email));

            if (search.IsActive.HasValue)
                query = query.Where(x => x.IsActive == search.IsActive.Value);

            return query;
        }

        protected override async Task BeforeInsertAsync(ApplicationUser entity, ApplicationUserInsertRequest request, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            // Ensure user is created through Identity
            var result = await _userManager.CreateAsync(entity, request.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        protected override async Task BeforeUpdateAsync(ApplicationUser entity, ApplicationUserUpdateRequest request, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(entity);
                var result = await _userManager.ResetPasswordAsync(entity, token, request.Password);
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
