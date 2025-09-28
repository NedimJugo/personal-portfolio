using Portfolio.Models.Requests.InsertRequests;
using Portfolio.Models.Requests.UpdateRequests;
using Portfolio.Models.Responses;
using Portfolio.Models.SearchObjects;
using Portfolio.Services.BaseInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Interfaces
{
    public interface IEmailTemplateService
        : ICRUDService<EmailTemplateResponse, EmailTemplateSearchObject, EmailTemplateInsertRequest, EmailTemplateUpdateRequest, Guid>
    {
    }
}
