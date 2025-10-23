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
    public class ContactMessageReplyProfile : Profile
    {
        public ContactMessageReplyProfile()
        {
            // Entity -> Response
            CreateMap<ContactMessageReply, ContactMessageReplyResponse>();

            // Insert -> Entity
            CreateMap<ContactMessageReplyInsertRequest, ContactMessageReply>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RepliedById, opt => opt.Ignore()) // Set in BeforeInsert
                .ForMember(dest => dest.RepliedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryStatus, opt => opt.MapFrom(_ => "sent"))
                .ForMember(dest => dest.DeliveredAt, opt => opt.Ignore())
                .ForMember(dest => dest.ErrorMessage, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedById, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ContactMessage, opt => opt.Ignore())
                .ForMember(dest => dest.RepliedBy, opt => opt.Ignore());

            // Update -> Entity
            CreateMap<ContactMessageReplyUpdateRequest, ContactMessageReply>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ContactMessageId, opt => opt.Ignore())
                .ForMember(dest => dest.RepliedById, opt => opt.Ignore())
                .ForMember(dest => dest.RepliedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedById, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ContactMessage, opt => opt.Ignore())
                .ForMember(dest => dest.RepliedBy, opt => opt.Ignore());
        }
    }
}
