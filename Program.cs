using Atlantis_Portfolio_Backend.Middleware;
using Atlantis_Portfolio_Backend_BLL;
using Atlantis_Portfolio_Backend_BLL.Services;
using Atlantis_Portfolio_Backend_DAL.Context;
using Atlantis_Portfolio_Backend_DAL.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                "https://localhost:3000",
                "https://abdallah-hameed-portfolio.runasp.net"
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlServer(
            builder.Configuration["AtlantisPortfolio:ConnectionStrings:DefaultConnection"]);
    }
    else
    {
        var dataFolder = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataFolder);
        var sqlitePath = Path.Combine(dataFolder, "atlantis.db");
        options.UseSqlite($"Data Source={sqlitePath}");
    }
});

builder.Services.AddScoped<clsUserService>();
builder.Services.AddScoped<clsTokenService>();
builder.Services.AddScoped<clsAuthService>();
builder.Services.AddScoped<clsProjectService>();
builder.Services.AddScoped<clsProjectSkillService>();
builder.Services.AddScoped<clsSkillService>();
builder.Services.AddScoped<clsPortfolioProfileService>();
builder.Services.AddScoped<clsSocialLinkService>();
builder.Services.AddScoped<clsEducationService>();
builder.Services.AddScoped<clsCourseService>();
builder.Services.AddScoped<clsActionService>();
builder.Services.AddScoped<clsAuditLogService>();


var jwtKey = builder.Configuration["AtlantisPortfolio:Jwt:Key"];
var jwtIssuer = builder.Configuration["AtlantisPortfolio:Jwt:Issuer"];
var jwtAudience = builder.Configuration["AtlantisPortfolio:Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "Atlantis Portfolio JWT configuration is missing or incomplete.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status401Unauthorized;

    options.AddPolicy("AuthRateLimitPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document),
                new List<string>()
            }
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.EnsureCreated();
        Console.WriteLine("[DB INIT] Database initialized successfully.");

        if (!db.Users.Any(u => u.Email == "admin1@gmail.com"))
        {
            var admin = new User
            {
                Email = "admin1@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
                IsActive = true,
                FailedLoginAttempts = 0,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(admin);
            db.SaveChanges();
            Console.WriteLine("[DB SEED] Admin user created: admin1@gmail.com");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB INIT FAILED] {ex.Message}");
    }
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.EnablePersistAuthorization();
    });
}

//app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("FrontendPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();