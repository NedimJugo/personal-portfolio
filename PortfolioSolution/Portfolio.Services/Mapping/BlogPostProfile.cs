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
    public class BlogPostProfile : Profile
    {
        public BlogPostProfile()
        {
            // Entity -> Response
            CreateMap<BlogPost, BlogPostResponse>();

            // Insert -> Entity
            CreateMap<BlogPostInsertRequest, BlogPost>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // handled by Guid.NewGuid()
                .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.ReadingTime, opt => opt.MapFrom(src => CalculateReadingTime(src.Content)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PublishedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false));

            // Update -> Entity
            CreateMap<BlogPostUpdateRequest, BlogPost>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ReadingTime, opt => opt.MapFrom(src => CalculateReadingTime(src.Content)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PublishedAt, opt => opt.Ignore());
        }

        private static int CalculateReadingTime(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return 0;

            var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var wordsPerMinute = 200; // avg reading speed
            return (int)Math.Ceiling((double)wordCount / wordsPerMinute);
        }
    }
}
