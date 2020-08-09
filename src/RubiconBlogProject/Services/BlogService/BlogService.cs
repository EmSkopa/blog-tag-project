using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rubicon.Contexts;
using Rubicon.Dtos.Blog;
using Rubicon.Models;
using Rubicon.Profiles.Helpers;
using Slugify;

namespace Rubicon.Services.BlogService
{
    public class BlogService : IBlogService
    {
        private readonly RubiconDBContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<BlogService> _logger;

        public BlogService(RubiconDBContext context, IMapper mapper, ILogger<BlogService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<BlogResponseDto>> AddNewBlog(BlogCreateComplexDto blogCreateComplexDto)
        {
            try
            {
                SlugHelper helper = new SlugHelper();
                var slugForBlog = helper.GenerateSlug(blogCreateComplexDto.Title);

                var blogForCheckOfExistance = await _context.Blogs.FirstOrDefaultAsync(b => b.Slug == slugForBlog);
                if (blogForCheckOfExistance != null)
                {
                    _logger.LogWarning($"There is already blog with slug = {slugForBlog}");
                    return new ServiceResponse<BlogResponseDto>(
                        StatusCodes.Status400BadRequest,
                        $"There is already blog with slug = {slugForBlog}"
                    );
                }

                List<Tag> tags = new List<Tag>();
                foreach (var tagDto in blogCreateComplexDto.Tags)
                {
                    var tag = _mapper.Map<Tag>(tagDto);
                    var tagExistanceChecking = await _context.Tags
                                .FirstOrDefaultAsync(t => t.TagDescription == tag.TagDescription);
                    if (tagExistanceChecking == null)
                    {
                        await _context.Tags.AddAsync(tag);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"Added new tag = {tag.TagDescription}");
                    }
                    tags.Add(tag);
                }

                var blogForAddDto = _mapper.Map<BlogCreateDto>(blogCreateComplexDto);
                var blogForAdd = _mapper.Map<Blog>(blogForAddDto);
                blogForAdd.Tags = tags;

                await _context.Blogs.AddAsync(blogForAdd);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Added new blog with title = {blogForAdd.Title}");

                BlogWithTags blogWithTags = new BlogWithTags()
                {
                    Blog = blogForAdd,
                    Tags = blogForAdd.Tags
                };

                return new ServiceResponse<BlogResponseDto>(_mapper.Map<BlogResponseDto>(blogWithTags));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server error occured while adding new blog");
                return new ServiceResponse<BlogResponseDto>(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server error occured while adding new blog"
                );
            }
        }

        public async Task<ServiceResponse<BlogResponseDto>> DeleteBlogBySlug(string slug)
        {
            try
            {
                var blogForDelete = await _context.Blogs.FirstOrDefaultAsync(b => b.Slug == slug);
                if (blogForDelete == null)
                {
                    _logger.LogWarning($"There is no blog with slug = {slug}");
                    return new ServiceResponse<BlogResponseDto>(
                        StatusCodes.Status404NotFound,
                        $"There is no blog with slug = {slug}"
                    );
                }

                _context.Blogs.Remove(blogForDelete);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully deleted blog with slug = {blogForDelete.Slug}");
                return new ServiceResponse<BlogResponseDto>(_mapper.Map<BlogResponseDto>(blogForDelete));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server error occured while deleting blog by slug");
                return new ServiceResponse<BlogResponseDto>(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server error occured while deleting blog by slug"
                );
            }
        }

        public async Task<ServiceResponse<BlogResponseDto>> GetBlogBySlug(string slug)
        {
            try
            {
                var blogForReturn = await _context.Blogs.FirstOrDefaultAsync(b => b.Slug == slug);
                if (blogForReturn == null)
                {
                    _logger.LogWarning($"There is no blog with slug = {slug}");
                    return new ServiceResponse<BlogResponseDto>(
                        StatusCodes.Status404NotFound,
                        $"There is no blog with slug = {slug}"
                    );
                }

                _logger.LogInformation($"Successfully get blog with slug = {blogForReturn.Slug}");
                return new ServiceResponse<BlogResponseDto>(_mapper.Map<BlogResponseDto>(blogForReturn));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server error occured while getting blog by slug");
                return new ServiceResponse<BlogResponseDto>(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server error occured while getting blog by slug"
                );
            }
        }

        public async Task<ServiceResponse<ICollection<BlogResponseDto>>> GetBlogs(string tagQuery)
        {
            try
            {
                var blogs = await _context.Blogs
                                    .OrderByDescending(b => b.CreatedAt)
                                    .ToListAsync();
                if (!blogs.Any())
                {
                    _logger.LogWarning("There is no blogs in database");
                    return new ServiceResponse<ICollection<BlogResponseDto>>(
                        StatusCodes.Status404NotFound,
                        "There is no blogs in database"
                    );
                }

                List<BlogResponseDto> blogsDto = new List<BlogResponseDto>();
                foreach(var blog in blogs)
                {
                    if (tagQuery == null)
                    {
                        blogsDto.Add(_mapper.Map<BlogResponseDto>(blog));
                    }
                    else
                    {
                        var blogTags = blog.Tags;
                        foreach(var blogTag in blogTags)
                        {
                            if (blogTag.TagDescription == tagQuery)
                            {
                                blogsDto.Add(_mapper.Map<BlogResponseDto>(blog));
                            }
                        }
                    }
                }

                _logger.LogInformation($"Successfully get all blogs");
                return new ServiceResponse<ICollection<BlogResponseDto>>(blogsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server error occured while getting all blogs by tag query");
                return new ServiceResponse<ICollection<BlogResponseDto>>(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server error occured while getting all blogs by tag query"
                );
            }
        }

        public async Task<ServiceResponse<BlogResponseDto>> UpdateBlogBySlug(string slug, BlogUpdateDto blogUpdateDto)
        {
            try
            {
                var blogForUpdate = await _context.Blogs.FirstOrDefaultAsync(b => b.Slug == slug);
                if (blogForUpdate == null)
                {
                    _logger.LogWarning("There is no blog in database");
                    return new ServiceResponse<BlogResponseDto>(
                        StatusCodes.Status404NotFound,
                        "There is no blog in database"
                    );
                }

                _mapper.Map(blogUpdateDto, blogForUpdate);
                if (blogUpdateDto.Title != null)
                {
                    SlugHelper helper = new SlugHelper();
                    blogForUpdate.Slug = helper.GenerateSlug(blogUpdateDto.Title);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully updated blog with slug = {blogForUpdate.Slug}");

                return new ServiceResponse<BlogResponseDto>(_mapper.Map<BlogResponseDto>(blogForUpdate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server error occured while updating blog by slug");
                return new ServiceResponse<BlogResponseDto>(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server error occured while updating blog by slug"
                );
            }
        }
    }
}