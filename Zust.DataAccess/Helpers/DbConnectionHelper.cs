using Microsoft.Extensions.Configuration;

namespace Zust.DataAccess.Helpers
{
    /// <summary>
    /// Resolves the PostgreSQL connection string from configuration / environment in a
    /// host-agnostic way so the same code works locally, on Render and on Supabase.
    /// </summary>
    public static class DbConnectionHelper
    {
        /// <summary>
        /// Resolution order:
        /// 1. <c>ConnectionStrings:Default</c> (env var <c>ConnectionStrings__Default</c> or appsettings).
        /// 2. <c>DATABASE_URL</c> in URI form (e.g. <c>postgresql://user:pass@host:5432/db</c>),
        ///    which is what Render/Supabase expose, converted to an Npgsql keyword string.
        /// </summary>
        public static string? Resolve(IConfiguration configuration)
        {
            var fromConnectionStrings = configuration.GetConnectionString(Constants.Default);
            if (!string.IsNullOrWhiteSpace(fromConnectionStrings))
            {
                return fromConnectionStrings;
            }

            var databaseUrl = configuration["DATABASE_URL"]
                              ?? Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrWhiteSpace(databaseUrl))
            {
                return ConvertUriToNpgsql(databaseUrl);
            }

            return null;
        }

        /// <summary>
        /// Converts a <c>postgres(ql)://</c> URI into an Npgsql keyword connection string
        /// and forces SSL, which Supabase requires.
        /// </summary>
        private static string ConvertUriToNpgsql(string databaseUrl)
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':', 2);
            var username = Uri.UnescapeDataString(userInfo[0]);
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
            var database = uri.AbsolutePath.TrimStart('/');
            var port = uri.Port > 0 ? uri.Port : 5432;

            return $"Host={uri.Host};Port={port};Database={database};Username={username};" +
                   $"Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }
    }
}
