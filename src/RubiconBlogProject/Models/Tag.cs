using System;

namespace Rubicon.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string TagDescription { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Guid BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}