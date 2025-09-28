using EcoChallenge.Models.SearchObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Services.BaseInterfaces;

namespace Portfolio.WebAPI.BaseContoller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BaseController<T, TSearch, TId> : ControllerBase
        where T : class
        where TSearch : BaseSearchObject, new()
        where TId : struct
    {
        protected readonly IService<T, TSearch, TId> _service;
        protected readonly ILogger<BaseController<T, TSearch, TId>> _logger;

        public BaseController(
            IService<T, TSearch, TId> service,
            ILogger<BaseController<T, TSearch, TId>> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [AllowAnonymous]
        public virtual async Task<ActionResult<Models.Responses.PagedResult<T>>> Get(
            [FromQuery] TSearch? search = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetAsync(search ?? new TSearch(), cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting items with search: {@Search}", search);
                return BadRequest("An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [AllowAnonymous]
        public virtual async Task<ActionResult<T>> GetById(TId id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (IsDefaultValue(id))
                    return BadRequest("Invalid ID");

                var result = await _service.GetByIdAsync(id, cancellationToken);
                if (result == null)
                    return NotFound($"Item with ID {id} not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting item by id: {Id}", id);
                return BadRequest("An error occurred while processing your request.");
            }
        }

        [HttpHead("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [AllowAnonymous]
        public virtual async Task<IActionResult> Exists(TId id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (IsDefaultValue(id))
                    return BadRequest();

                var exists = await _service.ExistsAsync(id, cancellationToken);
                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if item exists: {Id}", id);
                return BadRequest();
            }
        }

        protected virtual bool IsDefaultValue(TId id)
        {
            return EqualityComparer<TId>.Default.Equals(id, default(TId));
        }

        protected int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : null;
        }

        protected bool IsCurrentUserAdmin()
        {
            return User.IsInRole("Admin");
        }

        protected IEnumerable<string> GetCurrentUserRoles()
        {
            return User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);
        }
    }
}
