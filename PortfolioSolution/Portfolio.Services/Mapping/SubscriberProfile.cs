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
    public class SubscriberProfile : Profile
    {
        public SubscriberProfile()
        {
            // Entity -> Response
            CreateMap<Subscriber, SubscriberResponse>();

            // Insert -> Entity
            CreateMap<SubscriberInsertRequest, Subscriber>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SubscribedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UnsubscribedAt, opt => opt.Ignore());

            // Update -> Entity
            CreateMap<SubscriberUpdateRequest, Subscriber>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SubscribedAt, opt => opt.Ignore());
        }
    }
}
