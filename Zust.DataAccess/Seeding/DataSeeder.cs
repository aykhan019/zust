using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zust.Core.Concrete.EntityFramework;
using Zust.Entities.Models;

namespace Zust.DataAccess.Seeding
{
    /// <summary>
    /// Seeds a small, presentable set of demo users and posts so the app can be shown
    /// publicly without manual data entry. Provider-agnostic (works on PostgreSQL) and
    /// idempotent — it only inserts data when the relevant tables are empty.
    /// </summary>
    public static class DataSeeder
    {
        private const string DemoPassword = "Demo@1234";

        private static readonly (string UserName, string Email, string About, string Image)[] DemoUsers =
        {
            ("Aladdin Monroe", "aladdin@zust.demo", "Coffee, code and long walks through the city.",
                "/assets/images/user/defaultUserImage.png"),
            ("Maya Rivera", "maya@zust.demo", "Photographer. Always chasing the golden hour.",
                "/assets/images/user/defaultUserImage.png"),
            ("Liam Chen", "liam@zust.demo", "Building things on the web. Football on weekends.",
                "/assets/images/user/defaultUserImage.png"),
            ("Sofia Khan", "sofia@zust.demo", "Designer who loves clean UI and messy sketchbooks.",
                "/assets/images/user/defaultUserImage.png"),
        };

        private static readonly string[] DemoPosts =
        {
            "Just shipped a new feature on Zust 🚀 Loving how it turned out!",
            "Beautiful sunrise this morning. Days like this make everything worth it. ☀️",
            "Reading a great book this weekend — any recommendations for the next one?",
            "Coffee count today: 4. Productivity count: questionable. ☕",
            "Weekend hike done. The view from the top never disappoints. 🏔️",
            "Throwback to last summer's trip. Already planning the next adventure ✈️",
        };

        public static async Task SeedAsync(IServiceProvider services, ILogger logger)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var db = services.GetRequiredService<ZustDbContext>();

            // Seed demo users.
            if (!await userManager.Users.AnyAsync())
            {
                logger.LogInformation("Seeding demo users...");
                foreach (var (userName, email, about, image) in DemoUsers)
                {
                    var user = new User
                    {
                        UserName = userName,
                        Email = email,
                        EmailConfirmed = true,
                        ImageUrl = image,
                        AboutMe = about,
                        Birthday = new DateTime(1995, 1, 1),
                        Gender = "Prefer not to say",
                    };

                    var result = await userManager.CreateAsync(user, DemoPassword);
                    if (!result.Succeeded)
                    {
                        logger.LogWarning("Failed to create demo user {Email}: {Errors}",
                            email, string.Join("; ", result.Errors.Select(e => e.Description)));
                    }
                }
            }

            // Seed demo posts (one per demo post text, cycled across the demo users).
            if (db.Posts is not null && !await db.Posts.AnyAsync())
            {
                var users = await userManager.Users.ToListAsync();
                if (users.Count > 0)
                {
                    logger.LogInformation("Seeding demo posts...");
                    for (var i = 0; i < DemoPosts.Length; i++)
                    {
                        var author = users[i % users.Count];
                        db.Posts.Add(new Post
                        {
                            Id = Guid.NewGuid().ToString(),
                            Description = DemoPosts[i],
                            HasMediaContent = false,
                            IsVideo = false,
                            CreatedAt = DateTime.Now.AddDays(-i),
                            UserId = author.Id,
                        });
                    }

                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
