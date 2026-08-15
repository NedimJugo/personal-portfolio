using Microsoft.AspNetCore.Authorization;
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
    public class SubscribersController
        : BaseCRUDController<SubscriberResponse, SubscriberSearchObject, SubscriberInsertRequest, SubscriberUpdateRequest, Guid>
    {
        public SubscribersController(ISubscriberService service, ILogger<SubscribersController> logger)
            : base(service, logger)
        {
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public override async Task<ActionResult<SubscriberResponse>> Create(
           [FromBody] SubscriberInsertRequest request,
           CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(request.Website))
            {
                _logger.LogInformation("Dropped subscriber submission that filled the honeypot field");
                return Ok();
            }

            return await base.Create(request, cancellationToken);
        }
    }
}
