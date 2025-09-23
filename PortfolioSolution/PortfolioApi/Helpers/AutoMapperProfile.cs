using AutoMapper;
using PortfolioApi.Models.DTOs;
using PortfolioApi.Models.Entities;

namespace PortfolioApi.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Will be set manually

            // Project mappings
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.FeaturedMediaUrl, opt => opt.MapFrom(src => src.FeaturedMedia != null ? src.FeaturedMedia.Url : null))
                .ForMember(dest => dest.Techs, opt => opt.MapFrom(src => src.ProjectTechs.Select(pt => pt.Tech)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.ProjectTags.Select(pt => pt.Tag)));

            CreateMap<CreateProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.Views, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.FeaturedMedia, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTechs, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTags, opt => opt.Ignore());

            CreateMap<UpdateProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.Views, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
                .ForMember(dest => dest.FeaturedMedia, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTechs, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTags, opt => opt.Ignore());

            // Tech mappings
            CreateMap<Tech, TechDto>()
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconMedia != null ? src.IconMedia.Url : null));

            // Tag mappings
            CreateMap<Tag, TagDto>();

            // Media mappings
            CreateMap<Media, MediaDto>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url));

            // Contact message mappings
            CreateMap<ContactMessageDto, ContactMessage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HandledById, opt => opt.Ignore())
                .ForMember(dest => dest.HandledBy, opt => opt.Ignore());
        }
    }

    public class MediaDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTimeOffset UploadedAt { get; set; }
    }
}
