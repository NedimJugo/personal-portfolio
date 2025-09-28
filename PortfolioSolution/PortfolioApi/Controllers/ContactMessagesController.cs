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
    public class ContactMessagesController
        : BaseCRUDController<ContactMessageResponse, ContactMessageSearchObject, ContactMessageInsertRequest, ContactMessageUpdateRequest, Guid>
    {
        public ContactMessagesController(IContactMessageService service, ILogger<ContactMessagesController> logger)
            : base(service, logger)
        {
        }
    }
}
