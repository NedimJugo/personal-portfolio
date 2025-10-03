using AutoMapper;
using Portfolio.Models.Requests.InsertRequests;
using Portfolio.Models.Responses;
using Portfolio.Services.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Mapping
{
    public class BlogPostLikeProfile : Profile
    {
        public BlogPostLikeProfile()
        {
            // Entity -> Response
            CreateMap<BlogPostLike, BlogPostLikeResponse>();

            // Insert -> Entity
            CreateMap<BlogPostLikeInsertRequest, BlogPostLike>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // let DB/EF generate Guid
                .ForMember(dest => dest.LikedAt, opt => opt.Ignore()); // handled in BeforeInsertAsync
        }
    }
}
