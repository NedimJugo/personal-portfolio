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

        public override async Task<ApplicationUserResponse> CreateAsync(ApplicationUserInsertRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                await ValidateInsertAsync(request, cancellationToken);

                var user = new ApplicationUser
                {
                    UserName = request.UserName?.Trim(),
                    Email = request.Email?.Trim(),
                    FullName = request.FullName?.Trim(),
                    IsActive = request.IsActive,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                // Create user with password through Identity
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create user: {Errors}", errors);
                    throw new InvalidOperationException($"Failed to create user: {errors}");
                }

                _logger.LogInformation("User created successfully: {UserId}", user.Id);
                return _mapper.Map<ApplicationUserResponse>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        public override async Task<ApplicationUserResponse?> UpdateAsync(int id, ApplicationUserUpdateRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User not found for update: {UserId}", id);
                    return null;
                }

                await ValidateUpdateAsync(id, request, cancellationToken);

                // Update basic properties
                user.UserName = request.UserName?.Trim();
                user.Email = request.Email?.Trim();
                user.FullName = request.FullName?.Trim();
                user.IsActive = request.IsActive;
                user.UpdatedAt = DateTimeOffset.UtcNow;

                // Update user through Identity (this handles UserName and Email normalization)
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to update user {UserId}: {Errors}", id, errors);
                    throw new InvalidOperationException($"Failed to update user: {errors}");
                }

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    _logger.LogInformation("Updating password for user: {UserId}", id);

                    // Remove existing password
                    var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                    if (!removePasswordResult.Succeeded)
                    {
                        var errors = string.Join("; ", removePasswordResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to remove old password for user {UserId}: {Errors}", id, errors);
                        throw new InvalidOperationException($"Failed to remove old password: {errors}");
                    }

                    // Add new password
                    var addPasswordResult = await _userManager.AddPasswordAsync(user, request.Password.Trim());
                    if (!addPasswordResult.Succeeded)
                    {
                        var errors = string.Join("; ", addPasswordResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to set new password for user {UserId}: {Errors}", id, errors);
                        throw new InvalidOperationException($"Failed to set new password: {errors}");
                    }

                    _logger.LogInformation("Password updated successfully for user: {UserId}", id);
                }

                // Refresh user data from database to get latest state
                user = await _userManager.FindByIdAsync(id.ToString());

                _logger.LogInformation("User updated successfully: {UserId}", id);
                return _mapper.Map<ApplicationUserResponse>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                throw;
            }
        }

        public override async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User not found for deletion: {UserId}", id);
                    return false;
                }

                await ValidateDeleteAsync(id, cancellationToken);

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to delete user {UserId}: {Errors}", id, errors);
                    throw new InvalidOperationException($"Failed to delete user: {errors}");
                }

                _logger.LogInformation("User deleted successfully: {UserId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                throw;
            }
        }

        protected override async Task ValidateInsertAsync(ApplicationUserInsertRequest request, CancellationToken cancellationToken = default)
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this email already exists");
            }

            // Check if username already exists
            var existingUserName = await _userManager.FindByNameAsync(request.UserName);
            if (existingUserName != null)
            {
                throw new InvalidOperationException("A user with this username already exists");
            }

            // Validate password strength
            var passwordValidator = _userManager.PasswordValidators.FirstOrDefault();
            if (passwordValidator != null)
            {
                var tempUser = new ApplicationUser { UserName = request.UserName };
                var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, tempUser, request.Password);
                if (!passwordValidationResult.Succeeded)
                {
                    var errors = string.Join("; ", passwordValidationResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Password validation failed: {errors}");
                }
            }
        }

        protected override async Task ValidateUpdateAsync(int id, ApplicationUserUpdateRequest request, CancellationToken cancellationToken = default)
        {
            // Check if email is taken by another user
            var existingUserWithEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserWithEmail != null && existingUserWithEmail.Id != id)
            {
                throw new InvalidOperationException("A user with this email already exists");
            }

            // Check if username is taken by another user
            var existingUserWithUsername = await _userManager.FindByNameAsync(request.UserName);
            if (existingUserWithUsername != null && existingUserWithUsername.Id != id)
            {
                throw new InvalidOperationException("A user with this username already exists");
            }

            // Validate password if provided
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user != null)
                {
                    var passwordValidator = _userManager.PasswordValidators.FirstOrDefault();
                    if (passwordValidator != null)
                    {
                        var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, user, request.Password);
                        if (!passwordValidationResult.Succeeded)
                        {
                            var errors = string.Join("; ", passwordValidationResult.Errors.Select(e => e.Description));
                            throw new InvalidOperationException($"Password validation failed: {errors}");
                        }
                    }
                }
            }
        }

        protected override async Task ValidateDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            // Add any business rules for deletion
            // For example, prevent deleting the last admin user
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                {
                    var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                    if (adminUsers.Count <= 1)
                    {
                        throw new InvalidOperationException("Cannot delete the last admin user");
                    }
                }
            }
        }

        protected override Task BeforeInsertAsync(ApplicationUser entity, ApplicationUserInsertRequest request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        protected override Task BeforeUpdateAsync(ApplicationUser entity, ApplicationUserUpdateRequest request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
