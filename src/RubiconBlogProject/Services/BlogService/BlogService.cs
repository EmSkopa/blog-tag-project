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
using Rubicon.Dtos.Tag;
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

                var blogForCheckOfExistance = await _context.Blogs
                                            .Include(b => b.BlogTags)
                                                .ThenInclude(bt => bt.Tag)
                                            .FirstOrDefaultAsync(b => b.Slug == slugForBlog);
                if (blogForCheckOfExistance != null)
                {
                    _logger.LogWarning($"There is already blog with slug = {slugForBlog}");
                    return new ServiceResponse<BlogResponseDto>(
                        StatusCodes.Status400BadRequest,
                        $"There is already blog with slug = {slugForBlog}"
                    );
                }

                List<Tag> tags = new List<Tag>();
                foreach (var tagDto in blogCreateComplexDto.TagList)
                {
                    var tagExistanceChecking = await _context.Tags
                                .FirstOrDefaultAsync(t => t.TagDescription == tagDto);
                    if (tagExistanceChecking == null)
                    {
                        var tag = new Tag { TagDescription = tagDto };
                        await _context.Tags.AddAsync(tag);

                        tags.Add(tag);

                        _logger.LogInformation($"Added new tag = {tag.TagDescription}");
                    }
                    else
                    {
                        tags.Add(tagExistanceChecking);
                    }
                }

                var blogForAddDto = _mapper.Map<BlogCreateDto>(blogCreateComplexDto);
                var blogForAdd = _mapper.Map<Blog>(blogForAddDto);
                blogForAdd.Slug = slugForBlog;
                
                await _context.Blogs.AddAsync(blogForAdd);

                foreach(var tag in tags)
                {
                    var blogTag = new BlogTag
                    {
                        Blog = blogForAdd,
                        Tag = tag
                    };
                    await _context.BlogTags.AddAsync(blogTag);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Added new blog with title = {blogForAdd.Title}");
                var blogWithTags = _mapper.Map<BlogResponseDto>(blogForAdd);
                blogWithTags.TagList = blogCreateComplexDto.TagList;

                return new ServiceResponse<BlogResponseDto>(blogWithTags);
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
                var blogForReturn = await _context.Blogs
                                        .Include(b => b.BlogTags)
                                            .ThenInclude(bt => bt.Tag)
                                        .FirstOrDefaultAsync(b => b.Slug == slug);
                if (blogForReturn == null)
                {
                    _logger.LogWarning($"There is no blog with slug = {slug}");
                    return new ServiceResponse<BlogResponseDto>(
                        StatusCodes.Status404NotFound,
                        $"There is no blog with slug = {slug}"
                    );
                }

                _logger.LogInformation($"Successfully get blog with slug = {blogForReturn.Slug}");
                var blogForReturnDto = _mapper.Map<BlogResponseDto>(blogForReturn);
                List<string> tags = new List<string>();
                foreach(var blogTags in blogForReturn.BlogTags)
                {
                    tags.Add(blogTags.Tag.TagDescription);
                }
                blogForReturnDto.TagList = tags;
                return new ServiceResponse<BlogResponseDto>(blogForReturnDto);
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
                                    .Include(b => b.BlogTags)
                                        .ThenInclude(bt => bt.Tag)
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
                    if (String.IsNullOrEmpty(tagQuery))
                    {
                        var blogDto = _mapper.Map<BlogResponseDto>(blog);
                        List<string> tags = new List<string>();
                        foreach (var blogTag in blog.BlogTags)
                        {
                            tags.Add(blogTag.Tag.TagDescription);
                        }

                        blogDto.TagList = tags;

                        blogsDto.Add(blogDto);
                    }
                    else
                    {
                        var blogTags = blog.BlogTags;
                        foreach(var blogTag in blogTags)
                        {
                            if (blogTag.Tag.TagDescription == tagQuery)
                            {
                                var blogDto = _mapper.Map<BlogResponseDto>(blog);
                                blogDto.TagList = new List<string>() { blogTag.Tag.TagDescription };
                                blogsDto.Add(blogDto);
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
                var blogForUpdate = await _context.Blogs
                                    .Include(b => b.BlogTags)
                                        .ThenInclude(bt => bt.Tag)
                                    .FirstOrDefaultAsync(b => b.Slug == slug);
                if (blogForUpdate == null)
                {
                    _logger.LogWarning("There is no blog in database");
                    return new ServiceResponse<BlogResponseDto>(
                        StatusCodes.Status404NotFound,
                        "There is no blog in database"
                    );
                }

                if (String.IsNullOrEmpty(blogUpdateDto.Body) &&
                    String.IsNullOrEmpty(blogUpdateDto.Description) &&
                    String.IsNullOrEmpty(blogUpdateDto.Title))
                {
                    _logger.LogWarning("Didn't specify what you want to update");
                    return new ServiceResponse<BlogResponseDto>(
                        StatusCodes.Status400BadRequest,
                        "Didn't specify what you want to update"
                    );
                }
                
                if (String.IsNullOrEmpty(blogUpdateDto.Body))
                {
                    blogUpdateDto.Body = blogForUpdate.Body;
                }
                if (String.IsNullOrEmpty(blogUpdateDto.Description))
                {
                    blogUpdateDto.Description = blogForUpdate.Description;
                }

                var updateTitle = true;
                if (String.IsNullOrEmpty(blogUpdateDto.Title))
                {
                    blogUpdateDto.Title = blogForUpdate.Title;
                    updateTitle = false;
                }
                
                _mapper.Map(blogUpdateDto, blogForUpdate);
                if (updateTitle)
                {
                    SlugHelper helper = new SlugHelper();
                    blogForUpdate.Slug = helper.GenerateSlug(blogUpdateDto.Title);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully updated blog with slug = {blogForUpdate.Slug}");
                
                var blogForReturnDto = _mapper.Map<BlogResponseDto>(blogForUpdate);
                
                List<string> tags = new List<string>();
                foreach(var blogTags in blogForUpdate.BlogTags)
                {
                    tags.Add(blogTags.Tag.TagDescription);
                }
                blogForReturnDto.TagList = tags;
                
                return new ServiceResponse<BlogResponseDto>(blogForReturnDto);
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