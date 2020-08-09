using System.Collections.Generic;
using Rubicon.Models;

namespace Rubicon.Profiles.Helpers
{
    public class BlogWithTags
    {
        public Blog Blog { get; set; }
        public ICollection<Tag> Tags { get; set; }
    }
}