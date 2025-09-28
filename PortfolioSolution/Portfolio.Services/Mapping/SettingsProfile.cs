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
    public class SettingsProfile : Profile
    {
        public SettingsProfile()
        {
            // Entity -> Response
            CreateMap<Settings, SettingsResponse>();

            // Insert -> Entity
            CreateMap<SettingsInsertRequest, Settings>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Guid.NewGuid() handled by entity default
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Update -> Entity
            CreateMap<SettingsUpdateRequest, Settings>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}
