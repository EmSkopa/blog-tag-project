using System;
using System.Collections.Generic;

namespace Rubicon.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string TagDescription { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<BlogTag> BlogTags { get; set; }
    }
}