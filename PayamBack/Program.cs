using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PayamBack;
using PayamBack.Data;
using PayamBack.Filters;
using PayamBack.Models.Identity;
using PayamBack.Services.Implementations;
using PayamBack.Services.Interfaces;
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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddOpenApi();

// اضافه کردن فیلتر به همه کنترلرها
builder.Services.AddScoped<PermissionFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<PermissionFilter>();
});

// ============================================================
// 6️⃣ سرویس‌های پروژه
// ============================================================
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICaptchaService, CaptchaService>();
builder.Services.AddScoped<ISignatureService, SignatureService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IMarkazCacheService, MarkazCacheService>();
builder.Services.AddScoped<IAccessService, AccessService>();
builder.Services.AddScoped<ICacheManager, CacheManager>();
builder.Services.AddScoped<IPermissionCacheService, PermissionCacheService>();
builder.Services.AddScoped<ILookupCacheService, LookupCacheService>();

builder.Services.AddHttpContextAccessor();

// ============================================================
// 7️⃣ CORS برای React
// ============================================================
builder.Services.AddCors(opt => opt.AddPolicy("React", p =>
    p.WithOrigins(
        "http://localhost:5173",
        "http://localhost:3000",
        "http://94.74.170.19"   // ← آدرس سرور خودتان را اضافه کنید
    )
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("React");

// ============================================================
// 🔥 مهم: این سه خط را اضافه کنید
// ============================================================
app.UseDefaultFiles();           // ← ایندکس پیش‌فرض (index.html)
// ← سرو فایل‌های استاتیک از wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name.ToLower();
        var requestPath = ctx.Context.Request.Path.Value?.ToLower() ?? "";

        // 🔥 index.html هرگز کش نشود
        if (path == "index.html" || !requestPath.Contains("."))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Context.Response.Headers.Append("Pragma", "no-cache");
            ctx.Context.Response.Headers.Append("Expires", "0");
        }
        // 🔥 فایل‌های هش‌دار در assets کش طولانی‌مدت
        else if (requestPath.Contains("/assets/"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
        }
    }
});

app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// 🔥 Fallback برای React Router
// ============================================================
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