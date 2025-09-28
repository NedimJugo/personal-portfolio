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
    public class ProjectImageProfile : Profile
    {
        public ProjectImageProfile()
        {
            CreateMap<ProjectImage, ProjectImageResponse>();
            CreateMap<ProjectImageInsertRequest, ProjectImage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<ProjectImageUpdateRequest, ProjectImage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
