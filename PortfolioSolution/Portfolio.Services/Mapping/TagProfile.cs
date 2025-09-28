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
    public class TagProfile : Profile
    {
        public TagProfile()
        {
            // Entity -> Response
            CreateMap<Tag, TagResponse>();

            // Insert -> Entity
            CreateMap<TagInsertRequest, Tag>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Update -> Entity
            CreateMap<TagUpdateRequest, Tag>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
