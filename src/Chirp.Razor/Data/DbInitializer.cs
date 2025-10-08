using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Chirp.Razor.Models;

namespace Chirp.Razor.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ChirpDbContext context)
        {
            context.Database.Migrate();

            if (context.Authors.Any() && context.Cheeps.Any())
                return;

            Author EnsureAuthor(string name, string email)
            {
                var a = context.Authors.SingleOrDefault(x => x.Email == email);
                if (a == null)
                {
                    a = new Author { Name = name, Email = email };
                    context.Authors.Add(a);
                    context.SaveChanges();
                }
                return a;
            }

            var nicole = EnsureAuthor("Nicole", "nicole@itu.dk");

            void EnsureCheep(Author author, string text, DateTime utcTime)
            {
                bool exists = context.Cheeps.Any(c => c.AuthorId == author.Id && c.Text == text);
                if (!exists)
                {
                    context.Cheeps.Add(new Cheep
                    {
                        Text = text,
                        Timestamp = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc),
                        AuthorId = author.Id,
                        Author = author
                    });
                    context.SaveChanges();
                }
            }

            var now = DateTime.UtcNow;
            EnsureCheep(nicole, "Testing C:",  now);
        }
    }
}
