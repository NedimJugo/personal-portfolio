using EcoChallenge.Models.SearchObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Services.BaseInterfaces;

namespace Portfolio.WebAPI.BaseContoller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BaseCRUDController<T, TSearch, TInsert, TUpdate, TId>
        : BaseController<T, TSearch, TId>
        where T : class
        where TSearch : BaseSearchObject, new()
        where TInsert : class
        where TUpdate : class
        where TId : struct
    {
        protected new readonly ICRUDService<T, TSearch, TInsert, TUpdate, TId> _service;

        public BaseCRUDController(
            ICRUDService<T, TSearch, TInsert, TUpdate, TId> service,
            ILogger<BaseCRUDController<T, TSearch, TInsert, TUpdate, TId>> logger) : base(service, logger)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        public virtual async Task<ActionResult<T>> Create(
            [FromBody] TInsert request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.CreateAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = GetEntityId(result) }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating item: {@Request}", request);
                return BadRequest("An error occurred while processing your request.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public virtual async Task<ActionResult<T>> Update(
            TId id,
            [FromBody] TUpdate request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (IsDefaultValue(id))
                    return BadRequest("Invalid ID");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.UpdateAsync(id, request, cancellationToken);
                if (result == null)
                    return NotFound($"Item with ID {id} not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating item {Id}: {@Request}", id, request);
                return BadRequest("An error occurred while processing your request.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [Authorize(Roles = "Admin")]
        public virtual async Task<IActionResult> Delete(TId id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (IsDefaultValue(id))
                    return BadRequest("Invalid ID");

                var success = await _service.DeleteAsync(id, cancellationToken);
                if (!success)
                    return NotFound($"Item with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting item: {Id}", id);
                return BadRequest("An error occurred while processing your request.");
            }
        }

        protected virtual object GetEntityId(T entity)
        {
            var property = typeof(T).GetProperty("Id");
            return property?.GetValue(entity) ?? default(TId);
        }
    }
}
