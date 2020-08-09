using System;
using System.Collections.Generic;

namespace Rubicon.Models
{
    public class Blog
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Tag> Tags { get; set; }
    }
}