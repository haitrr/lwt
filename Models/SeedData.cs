using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;


namespace LWT.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new LWTContext(
                serviceProvider.GetRequiredService<DbContextOptions<LWTContext>>()
            ))
            {
                // Look for any text
                if(context.Text.Any())
                {
                    return;  // Database is seeded
                }

                // Seed language
                context.Language.AddRange(
                    new Language
                    {
                        Name = "English",
                        TextSize = 12,
                        WordSplitPattern = @"[A-Za-z]+(?:'[A-Za-z]+)*"
                    },
                    new Language
                    {
                        Name = "Vietnamese",
                        TextSize = 13,
                        WordSplitPattern = @"[A-Za-z]+(?:'[A-Za-z]+)*"
                    }
                );
                context.SaveChanges();
                // Seed text
                context.Text.AddRange(
                    new Text
                    {
                        Title = "Example text",
                        Content = "This is an example text, don't care about it :D",
                        Language = context.Language.Where(language => language.Name == "English").FirstOrDefault()
                    },
                    new Text
                    {
                        Title = "Example Vietnamese",
                        Content = "Tieng viet rat hay",
                        Language = context.Language.Where(language => language.Name == "Vietnamese").FirstOrDefault()
                    },
                    new Text
                    {
                        Title = "Example text in English 2",
                        Content = "This is an example text, don't care about it :D",
                        Language = context.Language.Where(language => language.Name == "English").FirstOrDefault()
                    },
                    new Text
                    {
                        Title = "Example Vietnamese 2",
                        Content = "Tieng viet rat hay",
                        Language = context.Language.Where(language => language.Name == "Vietnamese").FirstOrDefault()
                    }
                );
                context.SaveChanges();
            }
        }
    }
}