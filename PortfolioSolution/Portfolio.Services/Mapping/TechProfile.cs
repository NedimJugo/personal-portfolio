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
    public class TechProfile : Profile
    {
        public TechProfile()
        {
            // Entity -> Response
            CreateMap<Tech, TechResponse>()
                .ForMember(dest => dest.IconMedia, opt => opt.MapFrom(src => src.IconMedia));

            // Insert -> Entity
            CreateMap<TechInsertRequest, Tech>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Update -> Entity
            CreateMap<TechUpdateRequest, Tech>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
