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
    public class PageViewProfile : Profile
    {
        public PageViewProfile()
        {
            CreateMap<PageView, PageViewResponse>();

            CreateMap<PageViewInsertRequest, PageView>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ViewedAt, opt => opt.Ignore());

            CreateMap<PageViewUpdateRequest, PageView>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ViewedAt, opt => opt.Ignore());
        }
    }
}
