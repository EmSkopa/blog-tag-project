using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Rubicon.Contexts;
using Rubicon.Dtos.Blog;
using Rubicon.Dtos.Tag;
using Rubicon.Models;
using Rubicon.Profiles;
using Rubicon.Services.BlogService;
using Rubicon.Services.TagService;
using Xunit;

namespace RubiconBlogProject.Tests
{
    public class BlogServiceTests
    {
        private readonly Mock<BlogService> _sut;
        private readonly RubiconDBContext _context = new RubiconDBContext(
                new DbContextOptionsBuilder<RubiconDBContext>()
                    .UseInMemoryDatabase(databaseName: "RubiconBlogTestsDb").Options
        );
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<BlogService>> _loggerMock = new Mock<ILogger<BlogService>>();

        public BlogServiceTests()
        {
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new RubiconProfiles());
            });
            _mapper = mockMapper.CreateMapper();
            _sut = new Mock<BlogService>(MockBehavior.Default, _context, _mapper, _loggerMock.Object);
        }

        [Fact]
        public async Task GetBlogs_ShouldReturn404_IfThereIsNoBlogsInDb()
        {
            // arrange
            await ClearDB();
            
            // act
            var serviceResponse = await _sut.Object.GetBlogs("");
            
            // assert
            Assert.False(serviceResponse.Success);
            Assert.Null(serviceResponse.Data);
            Assert.Matches("There is no blogs in database", serviceResponse.Message.ToString());
        }

        [Fact]
        public async Task GetBlogs_ShouldReturnBlogs_IfThereIsBlogsInDb()
        {
            // arrange
            await ClearDB();
            await PrepareDb();
            
            // act
            var serviceResponse = await _sut.Object.GetBlogs("");
            List<BlogResponseDto> blogsCheck = (List<BlogResponseDto>)serviceResponse.Data;
            
            // assert
            var blogsDb = await _context.Blogs.ToListAsync();

            Assert.True(serviceResponse.Success);
            Assert.NotNull(serviceResponse.Data);
            Assert.Equal(blogsDb.Count, serviceResponse.Data.Count);
            Assert.Equal(blogsDb[2].Body, blogsCheck[0].Body);
            Assert.Equal(blogsDb[2].Description, blogsCheck[0].Description);
            Assert.Equal(blogsDb[2].Title, blogsCheck[0].Title);
            Assert.Equal(blogsDb[2].Tags.Count, blogsCheck[0].Tags.Count);
            Assert.Equal(blogsDb[1].Body, blogsCheck[1].Body);
            Assert.Equal(blogsDb[1].Description, blogsCheck[1].Description);
            Assert.Equal(blogsDb[1].Title, blogsCheck[1].Title);
            Assert.Equal(blogsDb[1].Tags.Count, blogsCheck[1].Tags.Count);
            Assert.Equal(blogsDb[0].Body, blogsCheck[2].Body);
            Assert.Equal(blogsDb[0].Description, blogsCheck[2].Description);
            Assert.Equal(blogsDb[0].Title, blogsCheck[2].Title);
            Assert.Equal(blogsDb[0].Tags.Count, blogsCheck[2].Tags.Count);
        }

        [Fact]
        public async Task GetBlogBySlug_ShouldReturn404_IfThereIsNoBlogInDb()
        {
            // arrange
            await ClearDB();
            
            // act
            var serviceResponse = await _sut.Object.GetBlogBySlug("title-1");
            
            // assert
            Assert.False(serviceResponse.Success);
            Assert.Null(serviceResponse.Data);
            Assert.Matches("There is no blog with slug = title-1", serviceResponse.Message.ToString());
        }

        [Fact]
        public async Task GetBlogBySlug_ShouldReturnBlog_IfThereIsBlogInDb()
        {
            // arrange
            await ClearDB();
            await PrepareDb();
            
            // act
            var serviceResponse = await _sut.Object.GetBlogBySlug("title-1");
            var blog = serviceResponse.Data;
            
            // assert
            var blogDb = await _context.Blogs.FirstOrDefaultAsync(b => b.Slug == "title-1");

            Assert.True(serviceResponse.Success);
            Assert.NotNull(serviceResponse.Data);
            Assert.Equal(blogDb.Body, blog.Body);
            Assert.Equal(blogDb.Description, blog.Description);
            Assert.Equal(blogDb.Title, blog.Title);
            Assert.Equal(blogDb.Tags.Count, blog.Tags.Count);
        }

        [Fact]
        public async Task AddNewBlog_ShouldReturn400_IfThereIsAlreadyBlogInDb()
        {
            // arrange
            await ClearDB();
            await PrepareDb();

            var blogCreate = new BlogCreateComplexDto()
            {
                Title = "title 1",
                Description = "description 1",
                Body = "body 1",
            };
            
            // act
            var serviceResponse = await _sut.Object.AddNewBlog(blogCreate);
            
            // assert
            Assert.False(serviceResponse.Success);
            Assert.Null(serviceResponse.Data);
            Assert.Matches("There is already blog with slug = title-1", serviceResponse.Message.ToString());
        }

        [Fact]
        public async Task AddNewBlog_ShouldAddNewBlog_IfThereIsNoBlogInDb()
        {
            // arrange
            await ClearDB();

            var tags = new List<TagDto>()
            {
                new TagDto { TagDescription = "IOS" },
                new TagDto { TagDescription = "Android" }
            };

            var blogCreate = new BlogCreateComplexDto()
            {
                Title = "title 1",
                Description = "description 1",
                Body = "body 1",
                Tags = tags
            };
            
            // act
            var serviceResponse = await _sut.Object.AddNewBlog(blogCreate);

            var tagsDb = await _context.Tags.ToListAsync();
            
            // assert
            Assert.True(serviceResponse.Success);
            Assert.NotNull(serviceResponse.Data);
            Assert.Equal(tags.Count, serviceResponse.Data.Tags.Count);
            Assert.Equal(tags.Count, tagsDb.Count);
            Assert.Equal(blogCreate.Title, serviceResponse.Data.Title);
            Assert.Equal(blogCreate.Description, serviceResponse.Data.Description);
            Assert.Equal(blogCreate.Body, serviceResponse.Data.Body);
        }

        [Fact]
        public async Task UpdateBlogBySlug_ShouldReturn404_IfThereIsNoBlogInDb()
        {
            // arrange
            await ClearDB();

            var blogUpdate = new BlogUpdateDto()
            {
                Title = "title 1",
                Description = "description 1",
                Body = "body 1",
            };
            
            // act
            var serviceResponse = await _sut.Object.UpdateBlogBySlug("title-1", blogUpdate);
            
            // assert
            Assert.False(serviceResponse.Success);
            Assert.Null(serviceResponse.Data);
            Assert.Matches("There is no blog in database", serviceResponse.Message.ToString());
        }

        [Fact]
        public async Task UpdateBlogBySlug_ShouldReturn400_IfUpdateParametersDidntSpecified()
        {
            // arrange
            await ClearDB();
            await PrepareDb();

            var blogUpdate = new BlogUpdateDto()
            {
            };
            
            // act
            var serviceResponse = await _sut.Object.UpdateBlogBySlug("title-1", blogUpdate);
            
            // assert
            Assert.False(serviceResponse.Success);
            Assert.Null(serviceResponse.Data);
            Assert.Matches("Didn't specify what you want to update", serviceResponse.Message.ToString());
        }

        [Fact]
        public async Task UpdateBlogBySlug_ShouldUpdateBlog_IfThereIsBlogInDb()
        {
            // arrange
            await ClearDB();
            await PrepareDb();

            var blogUpdate = new BlogUpdateDto()
            {
                Title = "title 8",
                Description = "description 8",
                Body = "body 8",
            };
            
            // act
            var serviceResponse = await _sut.Object.UpdateBlogBySlug("title-1", blogUpdate);
            
            // assert
            Assert.True(serviceResponse.Success);
            Assert.NotNull(serviceResponse.Data);
            Assert.Equal("title-8", serviceResponse.Data.Slug);
            Assert.Equal(blogUpdate.Title, serviceResponse.Data.Title);
            Assert.Equal(blogUpdate.Description, serviceResponse.Data.Description);
            Assert.Equal(blogUpdate.Body, serviceResponse.Data.Body);
        }

        [Fact]
        public async Task DeleteBlogBySlug_ShouldReturn404_IfThereIsNoBlogInDb()
        {
            // arrange
            await ClearDB();

            // act
            var serviceResponse = await _sut.Object.DeleteBlogBySlug("title-1");
            
            // assert
            Assert.False(serviceResponse.Success);
            Assert.Null(serviceResponse.Data);
            Assert.Matches("There is no blog with slug = title-1", serviceResponse.Message.ToString());
        }

        [Fact]
        public async Task DeleteBlogBySlug_ShouldDeleteBlog_IfThereIsBlogInDb()
        {
            // arrange
            await ClearDB();
            await PrepareDb();

            // act
            var blogsBefore = await _context.Blogs.ToListAsync();
            var serviceResponse = await _sut.Object.DeleteBlogBySlug("title-1");
            var blogsAfter = await _context.Blogs.ToListAsync();

            // assert
            Assert.True(serviceResponse.Success);
            Assert.NotNull(serviceResponse.Data);
            Assert.Equal(blogsBefore.Count - 1, blogsAfter.Count);
        }

        private async Task PrepareDb()
        {
            List<Tag> tags = new List<Tag>()
            {
                new Tag { TagDescription = "IOS" },
                new Tag { TagDescription = "Android" },
                new Tag { TagDescription = "Mac" }
            };

            List<Blog> blogs = new List<Blog>()
            {
                new Blog { Title = "title 1", Slug = "title-1", Body = "body 1", Description = "description 1", Tags = tags },
                new Blog { Title = "title 2", Slug = "title-2", Body = "body 2", Description = "description 2", Tags = tags },
                new Blog { Title = "title 3", Slug = "title-3", Body = "body 3", Description = "description 3", Tags = tags }
            };

            await _context.Tags.AddRangeAsync(tags);
            await _context.Blogs.AddRangeAsync(blogs);
            await _context.SaveChangesAsync();
        }
        private async Task ClearDB()
        {
            _context.Tags.RemoveRange(_context.Tags);
            _context.Blogs.RemoveRange(_context.Blogs);
            await _context.SaveChangesAsync();
        }
    }
}
