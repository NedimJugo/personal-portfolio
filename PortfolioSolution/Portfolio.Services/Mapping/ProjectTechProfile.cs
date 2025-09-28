using AutoMapper;
using Portfolio.Models.Requests.InsertRequests;
using Portfolio.Models.Requests.UpdateRequests;
using Portfolio.Models.Responses;
using Portfolio.Services.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Mapping
{
    public class ProjectTechProfile : Profile
    {
        public ProjectTechProfile()
        {
            CreateMap<ProjectTech, ProjectTechResponse>();
            CreateMap<ProjectTechInsertRequest, ProjectTech>();
            CreateMap<ProjectTechUpdateRequest, ProjectTech>();
        }
    }
}
