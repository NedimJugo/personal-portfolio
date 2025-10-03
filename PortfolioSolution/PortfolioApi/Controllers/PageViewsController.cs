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
    public class PageViewsController
        : BaseCRUDController<PageViewResponse, PageViewSearchObject, PageViewInsertRequest, PageViewUpdateRequest, Guid>
    {
        public PageViewsController(IPageViewService service, ILogger<PageViewsController> logger)
            : base(service, logger)
        {
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public override async Task<ActionResult<PageViewResponse>> Create(
            [FromBody] PageViewInsertRequest request,
            CancellationToken cancellationToken = default)
        {
            return await base.Create(request, cancellationToken);
        }
    }
}
