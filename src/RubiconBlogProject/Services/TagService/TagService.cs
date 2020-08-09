using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rubicon.Contexts;
using Rubicon.Dtos.Tag;

namespace Rubicon.Services.TagService
{
    public class TagService : ITagService
    {
        private readonly RubiconDBContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TagService> _logger;

        public TagService(RubiconDBContext context, IMapper mapper, ILogger<TagService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<ICollection<TagDto>>> GetTags()
        {
            try
            {
                var tags = await _context.Tags.ToListAsync();
                if (!tags.Any())
                {
                    _logger.LogWarning("There is no tags in database");
                    return new ServiceResponse<ICollection<TagDto>>(
                        StatusCodes.Status404NotFound, 
                        "There is no tags in database"
                    );
                }

                List<TagDto> tagsDto = new List<TagDto>();
                foreach(var tag in tags)
                {
                    tagsDto.Add(_mapper.Map<TagDto>(tag));
                }

                return new ServiceResponse<ICollection<TagDto>>(tagsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server error occurred while getting all tags from database");
                return new ServiceResponse<ICollection<TagDto>>(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server error occurred while getting all tags from database"
                );
            }
        }
    }
}