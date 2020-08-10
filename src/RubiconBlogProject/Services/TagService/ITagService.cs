using System.Collections.Generic;
using System.Threading.Tasks;
using Rubicon.Dtos.Tag;

namespace Rubicon.Services.TagService
{
    public interface ITagService
    {
        Task<ServiceResponse<ICollection<string>>> GetTags();
    }
}