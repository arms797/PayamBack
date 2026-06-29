using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PayamBack;
using PayamBack.Data;
using PayamBack.Middlewares;
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

    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
            {
                context.Token = token.Substring("Bearer ".Length).Trim();
            }
            return Task.CompletedTask;
        }
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

// ============================================================
// 8️⃣ HTTPS Redirection (فقط در محیط تولید)
// ============================================================
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("React");

// ============================================================
// 9️⃣ 🔥 میدلور مدیریت خطا (قبل از Authentication)
// ============================================================
app.UseMiddleware<GlobalExceptionMiddleware>();  // ← استفاده مستقیم

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============================================================
// 10️⃣ ایجاد داده‌های اولیه
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