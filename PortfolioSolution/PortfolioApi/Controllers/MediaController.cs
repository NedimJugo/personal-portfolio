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
    public class MediaController
        : BaseCRUDController<MediaResponse, MediaSearchObject, MediaInsertRequest, MediaUpdateRequest, Guid>
    {
        public MediaController(IMediaService service, ILogger<MediaController> logger)
            : base(service, logger)
        {
        }
    }
}
