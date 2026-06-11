using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zust.Core.Concrete.EntityFramework;

namespace Zust.DataAccess.Seeding
{
    /// <summary>
    /// Applies pending EF Core migrations on startup and, when explicitly enabled,
    /// seeds demo data. Designed to run safely on every deploy.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider rootServices, ILogger logger)
        {
            using var scope = rootServices.CreateScope();
            var services = scope.ServiceProvider;
            var configuration = services.GetRequiredService<IConfiguration>();

            try
            {
                var db = services.GetRequiredService<ZustDbContext>();

                logger.LogInformation("Applying database migrations...");
                await db.Database.MigrateAsync();

                // Seed demo data only when SEED_DEMO_DATA=true (e.g. for a portfolio demo).
                var seedFlag = configuration["SEED_DEMO_DATA"];
                if (bool.TryParse(seedFlag, out var seed) && seed)
                {
                    await DataSeeder.SeedAsync(services, logger);
                }
            }
            catch (Exception ex)
            {
                // Never print secrets; log the message only.
                logger.LogError(ex, "Database initialization failed: {Message}", ex.Message);
                throw;
            }
        }
    }
}
