using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;   // ← جدید
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using PayamBack;
using PayamBack.Data;
using PayamBack.Filters;
using PayamBack.Models.Identity;
using PayamBack.Services.Edu;
using PayamBack.Services.Implementations;
using PayamBack.Services.Interfaces;
using System.IO.Compression;                       // ← جدید
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1️⃣ DbContext
// ============================================================
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// 2️⃣ Identity
// ============================================================
builder.Services.AddIdentity<AppUser, AppRole>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ============================================================
// 3️⃣ In-Memory Cache
// ============================================================
builder.Services.AddMemoryCache();

// ============================================================
// 🔥 3.5 Response Compression (Gzip + Brotli)
// ============================================================
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] {
            "application/json",
            "application/javascript",
            "text/css",
            "text/html",
            "text/plain",
            "text/xml",
            "application/xml",
            "image/svg+xml"
        });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(opt =>
{
    opt.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(opt =>
{
    opt.Level = CompressionLevel.Fastest;
});

// ============================================================
// 4️⃣ JWT Authentication
// ============================================================
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!))
    };
});

builder.Services.AddAuthorization();

// ============================================================
// 5️⃣ تنظیم Json
// ============================================================
/*builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();//true;
    });*/

builder.Services.AddOpenApi();

builder.Services.AddScoped<PermissionFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<PermissionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
});

// ============================================================
// 6️⃣ سرویس‌های پروژه
// ============================================================
builder.Services.AddSingleton<TrackingMemoryCache>(sp =>
{
    var innerCache = new MemoryCache(new MemoryCacheOptions());
    return new TrackingMemoryCache(innerCache);
});

builder.Services.AddSingleton<IMemoryCache>(sp =>
    sp.GetRequiredService<TrackingMemoryCache>());

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICaptchaService, CaptchaService>();
builder.Services.AddScoped<ISignatureService, SignatureService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IMarkazCacheService, MarkazCacheService>();
builder.Services.AddScoped<IAccessService, AccessService>();
builder.Services.AddScoped<ICacheManager, CacheManager>();
builder.Services.AddSingleton<CacheVersionManager>();
builder.Services.AddScoped<IPermissionCacheService, PermissionCacheService>();
builder.Services.AddScoped<ILookupCacheService, LookupCacheService>();
builder.Services.AddScoped<IPermittedMarkazCacheService, PermittedMarkazCacheService>();

builder.Services.AddDarsCacheServices();

builder.Services.AddHttpContextAccessor();

// ============================================================
// 7️⃣ CORS برای React
// ============================================================
builder.Services.AddCors(opt => opt.AddPolicy("React", p =>
    p.WithOrigins(
        "http://localhost:5173",
        "http://localhost:3000",
        "http://94.74.170.19"
    )
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ============================================================
// 🔥 Response Compression — باید اول باشه
// ============================================================
app.UseResponseCompression();   // ← این خط اضافه شد

app.UseCors("React");

// ============================================================
// 🔥 سرو فایل‌های استاتیک
// ============================================================
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name.ToLower();
        var requestPath = ctx.Context.Request.Path.Value?.ToLower() ?? "";

        if (path == "index.html" || !requestPath.Contains("."))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Context.Response.Headers.Append("Pragma", "no-cache");
            ctx.Context.Response.Headers.Append("Expires", "0");
        }
        else if (requestPath.Contains("/assets/"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
        }
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapFallbackToFile("index.html");

app.MapControllers();

// ============================================================
// 8️⃣ ایجاد داده‌های اولیه
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "خطا در ایجاد داده‌های اولیه");
    }
}

app.Run();