using System.Collections.Generic;
using System.Threading.Tasks;
using Rubicon.Dtos.Blog;

namespace Rubicon.Services.BlogService
{
    public interface IBlogService
    {
        Task<ServiceResponse<ICollection<BlogResponseDto>>> GetBlogs(string tagQuery);
        Task<ServiceResponse<BlogResponseDto>> GetBlogBySlug(string slug);
        Task<ServiceResponse<BlogResponseDto>> AddNewBlog(BlogCreateComplexDto blogCreateComplexDto);
        Task<ServiceResponse<BlogResponseDto>> UpdateBlogBySlug(string slug, BlogUpdateDto blogUpdateDto);
        Task<ServiceResponse<BlogResponseDto>> DeleteBlogBySlug(string slug);
    }
}