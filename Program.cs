using System.Text;
using System.Threading.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using FluentValidation;
using FluentValidation.AspNetCore;
using KyInfo.Api.Validators;
using KyInfo.Infrastructure;
using KyInfo.Infrastructure.Options;
using KyInfo.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using KyInfo.Api.Middleware;
using KyInfo.Api.OpenApi;
using KyInfo.Api.Seed;
using KyInfo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
const string DevJwtKey = "KyInfo-Dev-ONLY-32char-minimum-secret-key!";

builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection(AdminSeedOptions.SectionName));

builder.Services.Configure<AiOptions>(builder.Configuration.GetSection(AiOptions.SectionName));
builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .PostConfigure(options =>
    {
        if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(options.Key))
        {
            options.Key = DevJwtKey;
        }
    })
    .Validate(options => !string.IsNullOrWhiteSpace(options.Key), "Jwt:Key 未配置。")
    .Validate(options => options.Key.Length >= 32, "Jwt:Key 长度至少 32 字符。")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer 未配置。")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience 未配置。")
    .Validate(options => options.ExpireMinutes > 0, "Jwt:ExpireMinutes 必须为正整数。")
    .ValidateOnStart();

builder.Services.AddHttpClient("OpenAi", (sp, client) =>
{
    var opts = sp.GetRequiredService<IOptionsMonitor<AiOptions>>().CurrentValue;
    var baseUrl = string.IsNullOrWhiteSpace(opts.BaseUrl) ? "https://api.openai.com/v1" : opts.BaseUrl.TrimEnd('/');
    client.BaseAddress = new Uri(baseUrl + "/");
    client.Timeout = TimeSpan.FromSeconds(120);
});

// Infrastructure
builder.Services.AddKyInfoInfrastructure(builder.Configuration);

// Application
builder.Services.AddKyInfoApplication();

// Controllers
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync("{\"message\":\"请求过于频繁，请稍后再试。\"}", token);
    };

    options.AddPolicy("auth_login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 15,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("auth_register", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("ai_chat", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("admin_upload", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("ratings_write", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "KyInfo API", Version = "v1" });
    options.OperationFilter<ErrorResponsesOperationFilter>();

    // Enable JWT auth in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "?????Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS (for Blazor frontend)
const string CorsPolicyName = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
        if (origins.Length == 0)
        {
            if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
            {
                throw new InvalidOperationException("Cors:Origins 未配置。非开发环境必须显式配置允许的前端来源。");
            }

            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            return;
        }

        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// JWT（Key 见 docs/CONFIGURATION.md；生产必须通过 Jwt__Key 等配置提供）
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    jwtOptions.Key = DevJwtKey;
}

var key = Encoding.UTF8.GetBytes(jwtOptions.Key);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 显式关闭入站 claim 映射，确保 JWT 里的短名称 claim（如 "sub"、"role"）
        // 不会被自动改写成 WS-Federation/ClaimTypes 形式。
        // 否则 token 虽然带了 role=Admin/Root，[Authorize(Roles = "...")] 仍可能匹配失败。
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            NameClaimType = JwtRegisteredClaimNames.UniqueName,
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await AdminSeeder.TrySeedAsync(app.Services, app.Environment);

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicyName);

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
