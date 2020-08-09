using AutoMapper;
using Rubicon.Dtos.Blog;
using Rubicon.Dtos.Tag;
using Rubicon.Models;
using Rubicon.Profiles.Helpers;

namespace Rubicon.Profiles
{
    public class RubiconProfiles : Profile
    {
        public RubiconProfiles()
        {
            CreateMap<BlogCreateComplexDto, BlogCreateDto>();
            CreateMap<BlogCreateDto, Blog>();
            CreateMap<TagDto, Tag>();
            CreateMap<Tag, TagDto>();
            CreateMap<Blog, BlogResponseDto>();
            CreateMap<BlogWithTags, BlogResponseDto>().IncludeMembers(b => b.Blog);
            CreateMap<BlogUpdateDto, Blog>();
        }
    }
}