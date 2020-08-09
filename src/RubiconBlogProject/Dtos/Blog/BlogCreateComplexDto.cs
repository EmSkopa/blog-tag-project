using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Rubicon.Dtos.Tag;

namespace Rubicon.Dtos.Blog
{
    public class BlogCreateComplexDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Body { get; set; }
        [Required]
        public ICollection<TagDto> Tags { get; set; }
    }
}