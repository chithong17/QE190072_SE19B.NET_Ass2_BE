using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.Interfaces;
using TaskTrack.Service.Implementations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System;

// Enable legacy timestamp behavior so UTC DateTimes can be mapped to PostgreSQL timestamp without time zone
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT Bearer Authentication support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskTrack API - Assignment 2 (QE190072)",
        Version = "v2",
        Description = "ASP.NET Core Web API with JWT Authentication and Role-based Authorization."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter JWT Bearer token format: Bearer <your_token>",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVercel", policy =>
    {
        var configuredFrontend = builder.Configuration["FRONTEND_URL"];
        policy.SetIsOriginAllowed(origin =>
               Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
               (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(configuredFrontend) && origin.Equals(configuredFrontend, StringComparison.OrdinalIgnoreCase))))
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Configure DbContext from Render's DATABASE_URL or the standard .NET
// ConnectionStrings:DefaultConnection setting.
var configuredConnection = builder.Configuration["DATABASE_URL"];
if (string.IsNullOrWhiteSpace(configuredConnection))
{
    configuredConnection = builder.Configuration.GetConnectionString("DefaultConnection");
}

if (string.IsNullOrWhiteSpace(configuredConnection))
{
    throw new InvalidOperationException(
        "Database connection is not configured. Set DATABASE_URL or ConnectionStrings__DefaultConnection.");
}

var databaseConnection = NormalizeDatabaseConnection(configuredConnection);
builder.Services.AddDbContext<TaskmanagementDbEgrzContext>(options =>
    options.UseNpgsql(databaseConnection));

// Configure JWT Authentication
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? builder.Configuration["Jwt:Secret"]
    ?? "TaskTrackSuperSecretSecurityKey_2026_PRN232_Assignment2_Default!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();

// Dependency Injection
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

// Auto-seed SystemAccount table and default Admin account on startup
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TaskmanagementDbEgrzContext>();
    var initSql = """
    CREATE TABLE IF NOT EXISTS "SystemAccount" (
        "AccountID"     SERIAL          PRIMARY KEY,
        "FullName"      VARCHAR(100)    NOT NULL,
        "Email"         VARCHAR(150)    NOT NULL UNIQUE,
        "PasswordHash"  VARCHAR(255)    NOT NULL,
        "Role"          SMALLINT        NOT NULL DEFAULT 0,
        "CreatedDate"   TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
        "RefreshToken"  VARCHAR(500)    NULL,
        "RefreshTokenExpiryTime" TIMESTAMP NULL
    );

    ALTER TABLE "SystemAccount" 
    ADD COLUMN IF NOT EXISTS "RefreshToken" VARCHAR(500) NULL,
    ADD COLUMN IF NOT EXISTS "RefreshTokenExpiryTime" TIMESTAMP NULL;

    ALTER TABLE "Task" 
    ADD COLUMN IF NOT EXISTS "CreatedByID" INT NULL,
    ADD COLUMN IF NOT EXISTS "UpdatedByID" INT NULL;

    DO $$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Task_CreatedBy') THEN
            ALTER TABLE "Task" ADD CONSTRAINT "FK_Task_CreatedBy" FOREIGN KEY ("CreatedByID") REFERENCES "SystemAccount" ("AccountID") ON DELETE RESTRICT;
        END IF;
        IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Task_UpdatedBy') THEN
            ALTER TABLE "Task" ADD CONSTRAINT "FK_Task_UpdatedBy" FOREIGN KEY ("UpdatedByID") REFERENCES "SystemAccount" ("AccountID") ON DELETE SET NULL;
        END IF;
    END $$;

    INSERT INTO "SystemAccount" ("FullName", "Email", "PasswordHash", "Role", "CreatedDate")
    VALUES (
        'System Administrator',
        'admin@tasktrack.com',
        '$2b$12$zChRFFhiP3GPQGFOR8jrX.x0tFssKY6F9QyIgSQq/8cdR242rVfei',
        1,
        CURRENT_TIMESTAMP
    )
    ON CONFLICT ("Email") DO NOTHING;
    """;
    db.Database.ExecuteSqlRaw(initSql);
}
catch (Exception ex)
{
    Console.WriteLine($"DB Auto-Init Warning: {ex.Message}");
}

// Keep Swagger available locally and after deployment for assignment review.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowVercel");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

app.Run();

static string NormalizeDatabaseConnection(string connection)
{
    if (!Uri.TryCreate(connection, UriKind.Absolute, out var uri) ||
        (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
    {
        return connection;
    }

    var credentials = uri.UserInfo.Split(':', 2);
    if (credentials.Length != 2)
    {
        throw new InvalidOperationException("DATABASE_URL does not contain valid credentials.");
    }

    var connectionBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
        Username = Uri.UnescapeDataString(credentials[0]),
        Password = Uri.UnescapeDataString(credentials[1]),
        SslMode = SslMode.Require
    };

    return connectionBuilder.ConnectionString;
}
