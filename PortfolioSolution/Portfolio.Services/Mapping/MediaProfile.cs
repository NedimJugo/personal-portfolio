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
    public class MediaProfile : Profile
    {
        public MediaProfile()
        {
            CreateMap<Media, MediaResponse>();

            CreateMap<MediaInsertRequest, Media>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedAt, opt => opt.Ignore());

            CreateMap<MediaUpdateRequest, Media>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedAt, opt => opt.Ignore());
        }
    }
}
