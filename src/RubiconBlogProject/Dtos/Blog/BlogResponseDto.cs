using System.Collections.Generic;
using Rubicon.Dtos.Tag;

namespace Rubicon.Dtos.Blog
{
    public class BlogResponseDto
    {
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
        public ICollection<string> TagList { get; set; }
    }
}