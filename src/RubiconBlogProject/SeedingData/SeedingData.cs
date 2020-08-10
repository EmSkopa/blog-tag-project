using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rubicon.Contexts;
using Rubicon.Models;

namespace Rubicon.SeedingData
{
    public static class SeedingData
    {
        public static void SeedInitiallyDataInDb(IApplicationBuilder applicationBuilder)
        {
            using(var serviceScoped = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScoped.ServiceProvider.GetService<RubiconDBContext>();
                context.Database.Migrate();
                
                if(!context.Blogs.Any() && !context.Tags.Any())
                {
                    
                    var tags = new List<Tag>()
                    {
                        new Tag { TagDescription = "IOS" },
                        new Tag { TagDescription = "Android" },
                        new Tag { TagDescription = "Mac" }
                    };

                    var blogs = new List<Blog>()
                    {
                        new Blog { Title = "title 1", Slug = "title-1", Body = "body 1", Description = "description 1", Tags = tags },
                        new Blog { Title = "title 2", Slug = "title-2", Body = "body 2", Description = "description 2", Tags = tags },
                        new Blog { Title = "title 3", Slug = "title-3", Body = "body 3", Description = "description 3", Tags = tags }
                    };
                    context.Tags.AddRange(tags);
                    context.Blogs.AddRange(blogs);
                    context.SaveChanges();
                }
            }
        }
    }
}