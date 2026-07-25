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
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1️⃣ DbContext
// ============================================================
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

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

// ============================================================
// 7️⃣ CORS برای React
// ============================================================
builder.Services.AddCors(opt => opt.AddPolicy("React", p =>
    p.WithOrigins("http://localhost:5173", "http://localhost:3000")
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("React");
app.UseAuthentication();

// ============================================================
// 🔥 Middleware برای دیباگ توکن و کاربر
// ============================================================
/*app.Use(async (context, next) =>
{
    // 1️⃣ لاگ کردن هدر Authorization
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    var token = authHeader?.Split(" ").Last();

    if (!string.IsNullOrEmpty(token))
    {
        Console.WriteLine($"🔍 [Middleware] Authorization header: {token.Substring(0, Math.Min(30, token.Length))}...");
    }
    else
    {
        Console.WriteLine("🔍 [Middleware] No Authorization header found");
    }

    // 2️⃣ ادامه به Middleware بعدی (Authentication)
    await next();

    // 3️⃣ بعد از احراز هویت، اطلاعات کاربر را لاگ کن
    var user = context.User;
    if (user?.Identity?.IsAuthenticated == true)
    {
        var userName = user.Identity.Name ?? "unknown";
        var roleClaims = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var activeRole = roleClaims.FirstOrDefault() ?? "No Role";

        Console.WriteLine($"🔍 [Middleware] After Auth - User: {userName}, Role: {activeRole}");
    }
    else
    {
        Console.WriteLine("🔍 [Middleware] User not authenticated");
    }
});
*/
app.UseAuthorization();
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