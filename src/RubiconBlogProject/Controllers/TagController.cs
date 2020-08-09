using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rubicon.Dtos.Tag;
using Rubicon.Services.TagService;

namespace Rubicon.Controllers
{
    [ApiController]
    [Route("api/tags")]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }
        
        [HttpGet]
        public async Task<ActionResult<ICollection<TagDto>>> GetAllTags()
        {
            var serviceResponse = await _tagService.GetTags();
            
            if (!serviceResponse.Success)
            {
                return StatusCode(serviceResponse.StatusCode, serviceResponse.Message);
            }

            var responseDtos = serviceResponse.Data;
            return Ok(responseDtos);
        }
    }
}