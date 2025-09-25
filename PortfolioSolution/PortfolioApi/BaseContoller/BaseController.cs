using EcoChallenge.Models.SearchObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Services.BaseInterfaces;

namespace Portfolio.WebAPI.BaseContoller
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class BaseController<T, TSearch> : ControllerBase
        where T : class
        where TSearch : BaseSearchObject, new()
    {
        protected readonly IService<T, TSearch> _service;
        protected readonly ILogger<BaseController<T, TSearch>> _logger;

        public BaseController(
            IService<T, TSearch> service,
            ILogger<BaseController<T, TSearch>> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get paginated list of items
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
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

        /// <summary>
        /// Get item by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public virtual async Task<ActionResult<T>> GetById(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("ID must be greater than 0");

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

        /// <summary>
        /// Check if item exists
        /// </summary>
        [HttpHead("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public virtual async Task<IActionResult> Exists(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (id <= 0)
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
    }
}
