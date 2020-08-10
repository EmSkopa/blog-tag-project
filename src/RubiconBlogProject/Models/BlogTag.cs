using System;

namespace Rubicon.Models
{
    public class BlogTag
    {
        public Guid BlogId { get; set; }
        public Blog Blog { get; set; }
        public Guid TagId { get; set; }
        public Tag Tag { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}