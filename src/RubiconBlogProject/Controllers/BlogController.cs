using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rubicon.Dtos.Blog;
using Rubicon.Services.BlogService;

namespace Rubicon.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogservice)
        {
            _blogService = blogservice;
        }
        
        [HttpGet]
        public async Task<ActionResult<ICollection<BlogResponseDto>>> GetAllBlogs([FromQuery]string tag)
        {
            var serviceResponse = await _blogService.GetBlogs(tag);
            
            if (!serviceResponse.Success)
            {
                return StatusCode(serviceResponse.StatusCode, serviceResponse.Message);
            }

            var responseDtos = serviceResponse.Data;
            return Ok(responseDtos);
        }
        
        [HttpGet("{slug}", Name = "GetBlogBySlug")]
        public async Task<ActionResult<BlogResponseDto>> GetBlogBySlug(string slug)
        {
            var serviceResponse = await _blogService.GetBlogBySlug(slug);
            
            if (!serviceResponse.Success)
            {
                return StatusCode(serviceResponse.StatusCode, serviceResponse.Message);
            }

            var responseDtos = serviceResponse.Data;
            return Ok(responseDtos);
        }
        
        [HttpPost]
        public async Task<ActionResult<BlogResponseDto>> AddNewBlog([FromBody]BlogCreateComplexDto blogCreateComplexDto)
        {
            var serviceResponse = await _blogService.AddNewBlog(blogCreateComplexDto);
            
            if (!serviceResponse.Success)
            {
                return StatusCode(serviceResponse.StatusCode, serviceResponse.Message);
            }

            var responseDtos = serviceResponse.Data;
            return CreatedAtRoute(nameof(GetBlogBySlug), new { slug = responseDtos.Slug}, responseDtos);
        }
        
        [HttpPut("{slug}")]
        public async Task<ActionResult<BlogResponseDto>> UpdateBlogBySlug(string slug, [FromBody]BlogUpdateDto blogUpdateDto)
        {
            var serviceResponse = await _blogService.UpdateBlogBySlug(slug, blogUpdateDto);
            
            if (!serviceResponse.Success)
            {
                return StatusCode(serviceResponse.StatusCode, serviceResponse.Message);
            }

            var responseDtos = serviceResponse.Data;
            return Ok(responseDtos);
        }
        
        [HttpDelete("{slug}")]
        public async Task<ActionResult<BlogResponseDto>> DeleteBlogBySlug(string slug)
        {
            var serviceResponse = await _blogService.DeleteBlogBySlug(slug);
            
            if (!serviceResponse.Success)
            {
                return StatusCode(serviceResponse.StatusCode, serviceResponse.Message);
            }

            return NoContent();
        }
    }
}