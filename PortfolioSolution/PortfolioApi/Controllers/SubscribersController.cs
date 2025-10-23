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
    [AllowAnonymous]
    public class SubscribersController
        : BaseCRUDController<SubscriberResponse, SubscriberSearchObject, SubscriberInsertRequest, SubscriberUpdateRequest, Guid>
    {
        public SubscribersController(ISubscriberService service, ILogger<SubscribersController> logger)
            : base(service, logger)
        {
        }
    }
}
