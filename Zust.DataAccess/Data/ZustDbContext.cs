using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Zust.DataAccess.Helpers;
using Zust.Entities.Models;

namespace Zust.Core.Concrete.EntityFramework
{
    /// <summary>
    /// Represents the DbContext for the Zust application, derived from IdentityDbContext with custom User, Role, and primary key type.
    /// </summary>
    public partial class ZustDbContext : IdentityDbContext<User, Role, string>
    {
        /// <summary>
        /// Initializes a new instance of the ZustDbContext class with the specified DbContextOptions.
        /// </summary>
        /// <param name="contextOptions">The options for configuring the DbContext.</param>
        public ZustDbContext(DbContextOptions<ZustDbContext> contextOptions) : base(contextOptions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ZustDbContext class.
        /// </summary>
        public ZustDbContext() 
        {
        }

        /// <summary>
        /// Connection string resolved once per process. Building a ConfigurationBuilder and
        /// reading the appsettings JSON files from disk is expensive, and OnConfiguring runs on
        /// every `new ZustDbContext()` — which the repositories do on every single query. Caching
        /// it turns each repository call from a disk-I/O + config-build into a cheap lookup.
        /// </summary>
        private static string? _cachedConnectionString;
        private static readonly object _connectionStringLock = new();

        /// <summary>
        /// Overrides the default configuration of the DbContext options.
        /// Sets the database connection based on the appsettings.json file.
        /// </summary>
        /// <param name="optionsBuilder">The DbContext options builder.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // This path is used by `new ZustDbContext()` (repositories) and at design time
                // (e.g. `dotnet ef migrations`). Runtime DI configuration is supplied in Program.cs.
                optionsBuilder.UseNpgsql(
                    GetConnectionString(),
                    npgsql => npgsql.EnableRetryOnFailure());
            }

            base.OnConfiguring(optionsBuilder);
        }

        /// <summary>
        /// Resolves the connection string once and caches it for the lifetime of the process.
        /// </summary>
        private static string GetConnectionString()
        {
            if (_cachedConnectionString is not null)
            {
                return _cachedConnectionString;
            }

            lock (_connectionStringLock)
            {
                if (_cachedConnectionString is not null)
                {
                    return _cachedConnectionString;
                }

                var configuration = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile(Constants.AppSettingsFile, optional: true, reloadOnChange: false)
                        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                        .AddEnvironmentVariables()
                        .Build();

                // Read the connection string (ConnectionStrings:Default or DATABASE_URL).
                // Falls back to a local placeholder so `migrations add` works without a live DB.
                _cachedConnectionString = DbConnectionHelper.Resolve(configuration)
                    ?? "Host=localhost;Port=5432;Database=zust;Username=postgres;Password=postgres";

                return _cachedConnectionString;
            }
        }

        /// <summary>
        /// Represents a DbSet for managing Friendships in the database.
        /// </summary>
        public DbSet<Friendship>? Friendships { get; set; }

        /// <summary>
        /// Represents a DbSet for managing Posts in the database.
        /// </summary>
        public DbSet<Post>? Posts { get; set; }

        /// <summary>
        /// Represents a DbSet for managing FriendRequests in the database.
        /// </summary>
        public DbSet<FriendRequest>? FriendRequest { get; set; }

        /// <summary>
        /// Represents a DbSet for managing Notifications in the database.
        /// </summary>
        public DbSet<Notification>? Notifications { get; set; }

        /// <summary>
        /// Represents a DbSet for managing Likes in the database.
        /// </summary>
        public DbSet<Like>? Likes { get; set; }

        /// <summary>
        /// Represents a DbSet for managing Comments in the database.
        /// </summary>
        public DbSet<Comment>? Comments { get; set; }

        /// <summary>
        /// Represents a DbSet for managing Chats in the database.
        /// </summary>
        public DbSet<Chat>? Chat { get; set; }

        /// <summary>
        /// Represents a DbSet for managing Messages in the database.
        /// </summary>
        public DbSet<Message>? Messages { get; set; }
    }
}
