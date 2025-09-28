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
    public class ProjectTagsController
        : BaseCRUDController<ProjectTagResponse, ProjectTagSearchObject, ProjectTagInsertRequest, ProjectTagUpdateRequest, Guid>
    {
        public ProjectTagsController(IProjectTagService service, ILogger<ProjectTagsController> logger)
            : base(service, logger)
        {
        }
    }
}
