using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Zust.Business.Abstract;
using Zust.Business.Concrete;
using Zust.Core.Concrete.EntityFramework;
using Zust.DataAccess.Abstract;
using Zust.DataAccess.Concrete.EFEntityFramework;
using Zust.DataAccess.Seeding;
using Zust.Entities.Models;
using Zust.Web.Abstract;
using Zust.Web.Concrete;
using Zust.Web.Helpers.ConstantHelpers;
using Zust.Web.Hubs;

// Npgsql 6+ maps DateTime to `timestamp with time zone` and rejects non-UTC values.
// The original code (written for SQL Server) stores local/unspecified DateTimes, so we
// opt into legacy behavior (`timestamp without time zone`) to preserve existing semantics.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Resolve the PostgreSQL connection string from configuration / environment.
// Priority: ConnectionStrings__Default  ->  DATABASE_URL (URI form, e.g. Neon/Render).
var connectionString = Zust.DataAccess.Helpers.DbConnectionHelper.Resolve(builder.Configuration);

builder.Services.AddDbContext<ZustDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Specify the assembly where the EF Core migrations are located
        npgsqlOptions.MigrationsAssembly(Constants.MigrationsAssembly);

        // Enable transient error resiliency (retry on failure)
        npgsqlOptions.EnableRetryOnFailure();
    });
});

// Dependency injection configuration
builder.Services.AddScoped<IUserDal, EFUserDal>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFriendRequestDal, EFFriendRequestDal>();
builder.Services.AddScoped<IFriendRequestService, FriendRequestService>();
builder.Services.AddScoped<INotificationDal, EFNotificationDal>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFriendshipDal,EFFriendshipDal>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IPostDal, EFPostDal>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IStaticService, StaticService>();
builder.Services.AddScoped<ILikeDal, EFLikeDal>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<ICommentDal, EFCommentDal>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IChatDal, EFChatDal>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IMessageDal, EFMessageDal>();
builder.Services.AddScoped<IMessageService, MessageService>();

// Register Session
builder.Services.AddSession();

// Register Identity. Usernames are full display names (e.g. "Aladdin Monroe"),
// so spaces must be permitted in the allowed username character set.
builder.Services.AddIdentity<User, Zust.Entities.Models.Role>(options =>
                {
                    options.User.AllowedUserNameCharacters += " ";
                })
                .AddEntityFrameworkStores<ZustDbContext>()
                .AddSignInManager<SignInManager<User>>()
                .AddDefaultTokenProviders();

// Register AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));
builder.Services.AddControllersWithViews();

// Register SignalR
builder.Services.AddSignalR();

// Configure the Identity application cookie (auth cookie used after sign-in).
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(Constants.CookieExpireTimeSpan);
    options.SlidingExpiration = true;
    // Behind Render's TLS-terminating proxy the request arrives as HTTPS via
    // forwarded headers, so the cookie can safely be marked Secure in production.
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// CORS — origins come from configuration/env (ALLOWED_ORIGINS, comma separated).
// The app is server-rendered and same-origin, so this only matters if you add an
// external frontend later. Defaults to the local HTTPS dev origin.
var allowedOrigins = (builder.Configuration["ALLOWED_ORIGINS"] ?? "https://localhost:7009")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Honor X-Forwarded-* headers from Render's reverse proxy so HTTPS detection,
// remote IP and generated URLs are correct in production.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Render's proxy is not in a known network/proxy list; clear the defaults so
    // forwarded headers are accepted.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configure the FormOptions to increase value and file size limits (media uploads).
builder.Services.Configure<FormOptions>(o => {
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = int.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
});

// Bind Kestrel to the port Render provides via the PORT environment variable.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

// Apply pending EF Core migrations (and seed demo data) on startup so a fresh
// Neon database is provisioned automatically on first deploy.
await DbInitializer.InitializeAsync(app.Services, app.Logger);

// Trust forwarded headers before anything that depends on scheme/host.
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Use Session
app.UseSession();

// HTTPS redirection is handled by Render's edge in production; only enforce it
// locally to avoid redirect loops behind the proxy.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

// Use the CORS policy (must sit between UseRouting and the endpoints).
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// Configure routes
app.MapControllerRoute(name: "Default", pattern: "{controller=Account}/{action=Landing}");
app.MapHub<UserHub>("/userhub");

app.Run();
