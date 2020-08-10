using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Rubicon.Contexts;
using Rubicon.Dtos.Tag;
using Rubicon.Models;
using Rubicon.Profiles;
using Rubicon.Services.TagService;
using Xunit;

namespace RubiconBlogProject.Tests
{
    public class TagServiceTests
    {
        private readonly Mock<TagService> _sut;
        private readonly RubiconDBContext _context = new RubiconDBContext(
                new DbContextOptionsBuilder<RubiconDBContext>()
                    .UseInMemoryDatabase(databaseName: "RubiconTagTestsDb").Options
        );
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<TagService>> _loggerMock = new Mock<ILogger<TagService>>();

        public TagServiceTests()
        {
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new RubiconProfiles());
            });
            _mapper = mockMapper.CreateMapper();
            _sut = new Mock<TagService>(MockBehavior.Default, _context, _mapper, _loggerMock.Object);
        }

        [Fact]
        public async Task GetTags_ShouldReturn404_IfThereIsNoTagsInDb()
        {
            // arrange
            await ClearDB();
            
            // act
            var serviceResponse = await _sut.Object.GetTags();
            
            // assert
            Assert.False(serviceResponse.Success);
            Assert.Null(serviceResponse.Data);
            Assert.Matches("There is no tags in database", serviceResponse.Message.ToString());
        }

        [Fact]
        public async Task GetTags_ShouldReturnTags_IfThereIsTagsInDb()
        {
            // arrange
            await ClearDB();
            await AddTagsInDb();
            
            // act
            var serviceResponse = await _sut.Object.GetTags();
            List<TagDto> tagsCheck = (List<TagDto>)serviceResponse.Data;
            
            // assert
            var tagsDb = await _context.Tags.ToListAsync();

            Assert.True(serviceResponse.Success);
            Assert.NotNull(serviceResponse.Data);
            Assert.Equal(tagsDb.Count, serviceResponse.Data.Count);
            Assert.Equal(tagsDb[0].TagDescription, tagsCheck[0].TagDescription);
            Assert.Equal(tagsDb[1].TagDescription, tagsCheck[1].TagDescription);
            Assert.Equal(tagsDb[2].TagDescription, tagsCheck[2].TagDescription);
        }

        private async Task AddTagsInDb()
        {
            List<Tag> tags = new List<Tag>()
            {
                new Tag { TagDescription = "IOS" },
                new Tag { TagDescription = "Android" },
                new Tag { TagDescription = "Mac" }
            };

            await _context.Tags.AddRangeAsync(tags);
            await _context.SaveChangesAsync();
        }
        private async Task ClearDB()
        {
            _context.Tags.RemoveRange(_context.Tags);
            await _context.SaveChangesAsync();
        }
    }
}
