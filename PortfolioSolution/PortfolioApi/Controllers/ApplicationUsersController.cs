using Microsoft.AspNetCore.Mvc;
using Portfolio.Models.Requests.InsertRequests;
using Portfolio.Models.Requests.UpdateRequests;
using Portfolio.Models.Responses;
using Portfolio.Models.SearchObjects;
using Portfolio.Services.Interfaces;
using Portfolio.WebAPI.BaseContoller;

namespace Portfolio.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUsersController
        : BaseCRUDController<ApplicationUserResponse, ApplicationUserSearchObject, ApplicationUserInsertRequest, ApplicationUserUpdateRequest, int>
    {
        public ApplicationUsersController(IApplicationUserService service, ILogger<ApplicationUsersController> logger)
            : base(service, logger)
        {
        }

        public override async Task<ActionResult<ApplicationUserResponse>> Update(
    int id,
    [FromBody] ApplicationUserUpdateRequest request,
    CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Update request received for user {UserId}", id);
                _logger.LogInformation("Request payload: {@Request}", new
                {
                    request.UserName,
                    request.Email,
                    request.FullName,
                    request.IsActive,
                    HasPassword = !string.IsNullOrEmpty(request.Password)
                });

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model state is invalid: {@Errors}",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                    return BadRequest(new
                    {
                        message = "Validation failed",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var result = await _service.UpdateAsync(id, request, cancellationToken);
                if (result == null)
                {
                    _logger.LogWarning("User not found: {UserId}", id);
                    return NotFound(new { message = $"User with id {id} not found" });
                }

                _logger.LogInformation("User updated successfully: {UserId}", id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business logic error updating user {UserId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the user" });
            }
        }

    }
}
