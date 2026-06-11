using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zust.Core.Concrete.EntityFramework;
using Zust.Entities.Models;

namespace Zust.DataAccess.Seeding
{
    /// <summary>
    /// Generates a large, realistic demo dataset across every table so the app feels
    /// populated. Idempotent — it only runs when the users table is empty. Avatars are
    /// generated (the original IPFS avatar links are dead); post/cover images reuse the
    /// project's still-live Cloudinary URLs.
    /// </summary>
    public static class DataSeeder
    {
        private const string DemoPassword = "Demo@1234";

        // ===== Volume knob =====
        // Everything below scales off UserCount. ~2000 users produces on the order of
        // ~700k total rows (≈ 40-50% of Neon's 0.5 GB free tier) and seeds in a few
        // minutes. Lower it (e.g. 100) for a quick seed; raise it to fill more storage,
        // but leave headroom so inserts don't hit the storage cap mid-run.
        private const int UserCount = 2000;

        // Rows per SaveChanges flush. Keeps the change tracker and memory bounded.
        private const int BatchSize = 5000;

        private static readonly Random Rnd = new(42);

        // Known accounts you can log in with (password: Demo@1234).
        private static readonly (string Name, string Email)[] NamedUsers =
        {
            ("Aladdin Monroe", "aladdin@zust.demo"),
            ("Maya Rivera", "maya@zust.demo"),
            ("Liam Chen", "liam@zust.demo"),
            ("Sofia Khan", "sofia@zust.demo"),
        };

        // 12 cover images (live Cloudinary URLs).
        private static readonly string[] Covers =
        {
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689144003/cover7_istccu.png",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689143999/cover9_hyqckl.png",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689143999/cover12_k0r8gp.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689143999/cover2_op3tmy.jpg",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689143999/cover5_xrb7yd.png",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689143999/cover10_jf2maa.jpg",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689143998/cover8_sf11xp.jpg",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689143998/cover11_pkvwq5.jpg",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689143998/cover4_h9cqsx.jpg",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689143998/cover1_qebrwn.jpg",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689143998/cover6_jsshqi.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689143998/cover3_q9zaqi.webp",
        };

        // 20 status images (live Cloudinary URLs).
        private static readonly string[] Statuses =
        {
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692145/status1_e48gre.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692145/status2_mrbsnv.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692145/status3_miemvi.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692145/status4_epyqxs.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692145/status5_jyxkxf.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692494/status6_hisvbw.jpg",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692145/status7_cuk7f0.jpg",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692144/status6_u7pan1.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692145/status9_mahqmb.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692144/status10_iji57x.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692144/status11_jegjp9.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692145/status12_rbso4j.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692144/status13_kszocd.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692144/status14_esabah.jpg",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692144/status15_vmsukw.jpg",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692144/status16_p8ytjv.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692144/status17_hykua7.webp",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692373/status18_cuszl4.jpg",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689692373/status19_px00gj.jpg",
            "https://res.cloudinary.com/dax9yhk8g/image/upload/v1689691497/status20_gzm5cr.webp",
        };

        // Pool of images attached to posts (covers + statuses, all reachable).
        private static readonly string[] PostImages = Covers.Concat(Statuses).ToArray();

        private static readonly string[] FirstNames =
        {
            "Alex", "Maria", "John", "Emma", "Daniel", "Olivia", "Lucas", "Sophia", "Liam", "Ava",
            "Noah", "Isabella", "Mateo", "Mia", "Ethan", "Amelia", "Leo", "Zoe", "Adam", "Lily",
            "Omar", "Nina", "Hugo", "Clara", "Ivan", "Elif", "Kai", "Yara", "Theo", "Aisha",
        };

        private static readonly string[] LastNames =
        {
            "Monroe", "Rivera", "Chen", "Khan", "Smith", "Johnson", "Garcia", "Kim", "Novak", "Silva",
            "Hassan", "Lopez", "Walker", "Yilmaz", "Petrov", "Nakamura", "Cohen", "Dubois", "Rossi", "Adams",
        };

        private static readonly string[] Occupations =
        {
            "Software Engineer", "Photographer", "Designer", "Teacher", "Doctor", "Writer",
            "Marketing Lead", "Student", "Chef", "Architect", "Musician", "Entrepreneur",
        };

        private static readonly string[] Cities =
        {
            "London", "New York", "Berlin", "Tokyo", "Istanbul", "Madrid", "Toronto",
            "Sydney", "Dubai", "Paris", "Baku", "Amsterdam",
        };

        private static readonly string[] Genders = { "Male", "Female", "Prefer not to say" };
        private static readonly string[] Relationships = { "Single", "In a relationship", "Married", "It's complicated" };
        private static readonly string[] BloodGroups = { "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" };
        private static readonly string[] LanguagesPool = { "English", "Spanish, English", "Turkish, English", "French", "German, English", "Japanese" };
        private static readonly string[] Interests = { "Coding, Coffee", "Photography, Travel", "Music, Reading", "Football, Gaming", "Art, Hiking", "Cooking, Cinema" };

        private static readonly string[] Abouts =
        {
            "Coffee, code and long walks through the city.",
            "Always chasing the golden hour.",
            "Building things on the web. Football on weekends.",
            "Designer who loves clean UI and messy sketchbooks.",
            "Just here for the memes and good conversations.",
            "Travel addict. 23 countries and counting.",
            "Trying to be 1% better every day.",
            "Dog person. Tea over coffee. Sometimes both.",
        };

        private static readonly string[] EducationWork =
        {
            "Studied Computer Science. Now building products.",
            "Self-taught and proud of it.",
            "MA in Design, currently freelancing.",
            "Engineering grad working in fintech.",
        };

        private static readonly string[] PostTexts =
        {
            "Just shipped a new feature 🚀 Loving how it turned out!",
            "Beautiful sunrise this morning. ☀️",
            "Any book recommendations for the weekend?",
            "Coffee count today: 4. Productivity: questionable. ☕",
            "Weekend hike done. The view never disappoints. 🏔️",
            "Throwback to last summer's trip. ✈️",
            "Golden hour never misses. 📸",
            "New workspace setup is finally done. 💻",
            "Nature therapy > everything. 🌿",
            "Some moments are worth slowing down for. ✨",
            "City lights and late nights. 🌆",
            "Grateful for days like these. 🙌",
            "Trying out a new recipe tonight. Wish me luck 🍳",
            "Monday motivation: just start. 💪",
            "Caught the sunset right on time. 🌅",
        };

        private static readonly string[] CommentTexts =
        {
            "This is amazing! 🔥", "Love this 😍", "So true.", "Great shot!", "Where is this?",
            "Congrats! 🎉", "Haha same here", "Need this energy", "Beautiful 😻", "Goals 💯",
            "Thanks for sharing", "Couldn't agree more", "Inspiring stuff", "On my way 😄", "Iconic.",
        };

        private static readonly string[] MessageTexts =
        {
            "Hey! How are you?", "Did you see that post? 😂", "Let's catch up this week",
            "Sounds good to me 👍", "Haha that's hilarious", "Where are you right now?",
            "Call you in 5", "Thanks a lot!", "See you tomorrow", "No worries at all",
            "That's awesome news 🎉", "I'll send it over now", "Long time no talk!", "Miss you!",
        };

        private static readonly string[] NotificationMessages =
        {
            "liked your post", "commented on your post", "sent you a friend request",
            "accepted your friend request", "started following you", "mentioned you in a comment",
        };

        public static async Task SeedAsync(IServiceProvider services, ILogger logger)
        {
            var db = services.GetRequiredService<ZustDbContext>();

            if (await db.Users.AnyAsync())
            {
                logger.LogInformation("Database already has users — skipping seed.");
                return;
            }

            db.ChangeTracker.AutoDetectChangesEnabled = false;
            logger.LogInformation("Seeding demo data for {UserCount} users (this can take a few minutes)...", UserCount);

            // --- Users ---
            var sharedHash = new PasswordHasher<User>().HashPassword(new User(), DemoPassword);
            var users = new List<User>(UserCount);
            for (var i = 0; i < UserCount; i++)
            {
                var id = Guid.NewGuid().ToString();
                string name, email, userName;
                if (i < NamedUsers.Length)
                {
                    name = NamedUsers[i].Name;
                    email = NamedUsers[i].Email;
                    userName = name;
                }
                else
                {
                    name = $"{Pick(FirstNames)} {Pick(LastNames)}";
                    userName = $"{name} {i}";          // suffix guarantees the unique username index
                    email = $"user{i}@zust.demo";
                }

                users.Add(new User
                {
                    Id = id,
                    UserName = userName,
                    NormalizedUserName = userName.ToUpperInvariant(),
                    Email = email,
                    NormalizedEmail = email.ToUpperInvariant(),
                    EmailConfirmed = true,
                    PasswordHash = sharedHash,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    LockoutEnabled = true,
                    ImageUrl = $"https://api.dicebear.com/9.x/avataaars/png?seed={id}",
                    CoverImage = Pick(Covers),
                    Birthday = new DateTime(1980, 1, 1).AddDays(Rnd.Next(0, 365 * 25)),
                    Occupation = Pick(Occupations),
                    Birthplace = Pick(Cities),
                    Gender = Pick(Genders),
                    RelationshipStatus = Pick(Relationships),
                    BloodGroup = Pick(BloodGroups),
                    Website = "https://example.com",
                    SocialLink = "https://twitter.com/zust",
                    Languages = Pick(LanguagesPool),
                    AboutMe = Pick(Abouts),
                    EducationWork = Pick(EducationWork),
                    Interests = Pick(Interests),
                });
            }
            await BulkInsertAsync(db, users, logger, "users");
            var userIds = users.Select(u => u.Id!).ToList();
            users = null!; // release

            // --- Posts (keep owner map for notifications) ---
            var posts = new List<Post>();
            var postIds = new List<string>();
            var postOwner = new Dictionary<string, string>();
            foreach (var uid in userIds)
            {
                var n = Rnd.Next(2, 16);
                for (var p = 0; p < n; p++)
                {
                    var pid = Guid.NewGuid().ToString();
                    var hasImage = Rnd.Next(100) < 80;
                    posts.Add(new Post
                    {
                        Id = pid,
                        Description = Pick(PostTexts),
                        ContentUrl = hasImage ? Pick(PostImages) : null,
                        HasMediaContent = hasImage,
                        IsVideo = false,
                        CreatedAt = RandomRecentDate(180),
                        UserId = uid,
                    });
                    postIds.Add(pid);
                    postOwner[pid] = uid;
                }
            }
            await BulkInsertAsync(db, posts, logger, "posts");
            posts = null!;

            // --- Comments ---
            var comments = new List<Comment>();
            foreach (var pid in postIds)
            {
                var m = Rnd.Next(0, 13);
                for (var c = 0; c < m; c++)
                {
                    comments.Add(new Comment
                    {
                        Id = Guid.NewGuid().ToString(),
                        PostId = pid,
                        UserId = Pick(userIds),
                        Text = Pick(CommentTexts),
                    });
                }
            }
            await BulkInsertAsync(db, comments, logger, "comments");
            comments = null!;

            // --- Likes (distinct user per post) ---
            var likes = new List<Like>();
            foreach (var pid in postIds)
            {
                var k = Rnd.Next(0, 36);
                var seen = new HashSet<string>();
                for (var l = 0; l < k; l++)
                {
                    var uid = Pick(userIds);
                    if (!seen.Add(uid))
                    {
                        continue;
                    }

                    likes.Add(new Like { Id = Guid.NewGuid().ToString(), PostId = pid, UserId = uid });
                }
            }
            await BulkInsertAsync(db, likes, logger, "likes");
            likes = null!;

            // --- Friendships (bidirectional, de-duplicated) ---
            var friendships = new List<Friendship>();
            var pairSeen = new HashSet<string>();
            foreach (var uid in userIds)
            {
                var f = Rnd.Next(8, 35);
                for (var j = 0; j < f; j++)
                {
                    var other = Pick(userIds);
                    if (other == uid)
                    {
                        continue;
                    }

                    var key = string.CompareOrdinal(uid, other) < 0 ? $"{uid}|{other}" : $"{other}|{uid}";
                    if (!pairSeen.Add(key))
                    {
                        continue;
                    }

                    friendships.Add(new Friendship { FriendshipId = Guid.NewGuid().ToString(), UserId = uid, FriendId = other });
                    friendships.Add(new Friendship { FriendshipId = Guid.NewGuid().ToString(), UserId = other, FriendId = uid });
                }
            }
            await BulkInsertAsync(db, friendships, logger, "friendships");
            friendships = null!;

            // --- Friend requests (pending) ---
            var requests = new List<FriendRequest>();
            for (var i = 0; i < UserCount * 4; i++)
            {
                var sender = Pick(userIds);
                var receiver = Pick(userIds);
                if (sender == receiver)
                {
                    continue;
                }

                requests.Add(new FriendRequest
                {
                    Id = Guid.NewGuid().ToString(),
                    SenderId = sender,
                    ReceiverId = receiver,
                    RequestDate = RandomRecentDate(60),
                    Status = "Pending",
                });
            }
            await BulkInsertAsync(db, requests, logger, "friend requests");
            requests = null!;

            // --- Notifications ---
            var notifications = new List<Notification>();
            foreach (var uid in userIds)
            {
                var q = Rnd.Next(3, 18);
                for (var k = 0; k < q; k++)
                {
                    notifications.Add(new Notification
                    {
                        Id = Guid.NewGuid().ToString(),
                        FromUserId = Pick(userIds),
                        ToUserId = uid,
                        Message = Pick(NotificationMessages),
                        IsRead = Rnd.Next(100) < 60,
                        Date = RandomRecentDate(45),
                    });
                }
            }
            await BulkInsertAsync(db, notifications, logger, "notifications");
            notifications = null!;

            // --- Chats + Messages ---
            var chats = new List<Chat>();
            var messages = new List<Message>();
            for (var i = 0; i < UserCount * 3; i++)
            {
                var a = Pick(userIds);
                var b = Pick(userIds);
                if (a == b)
                {
                    continue;
                }

                var chatId = Guid.NewGuid().ToString();
                chats.Add(new Chat { Id = chatId, SenderUserId = a, ReceiverUserId = b });

                var count = Rnd.Next(3, 40);
                for (var m = 0; m < count; m++)
                {
                    var fromA = Rnd.Next(2) == 0;
                    messages.Add(new Message
                    {
                        Id = Guid.NewGuid().ToString(),
                        ChatId = chatId,
                        SenderUserId = fromA ? a : b,
                        ReceiverUserId = fromA ? b : a,
                        Text = Pick(MessageTexts),
                        DateSent = RandomRecentDate(30),
                    });
                }
            }
            await BulkInsertAsync(db, chats, logger, "chats");
            chats = null!;
            await BulkInsertAsync(db, messages, logger, "messages");
            messages = null!;

            logger.LogInformation("Demo data seeding complete.");
        }

        private static T Pick<T>(IReadOnlyList<T> items) => items[Rnd.Next(items.Count)];

        private static DateTime RandomRecentDate(int withinDays) =>
            DateTime.Now.AddDays(-Rnd.Next(0, withinDays)).AddMinutes(-Rnd.Next(0, 1440));

        private static async Task BulkInsertAsync<T>(ZustDbContext db, List<T> items, ILogger logger, string label)
            where T : class
        {
            var total = items.Count;
            for (var i = 0; i < total; i += BatchSize)
            {
                var chunk = items.GetRange(i, Math.Min(BatchSize, total - i));
                await db.Set<T>().AddRangeAsync(chunk);
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();
            }

            logger.LogInformation("  seeded {Count} {Label}", total, label);
        }
    }
}
