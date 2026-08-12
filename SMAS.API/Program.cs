using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using SMAS.API.Data;
using SMAS.API.Middleware;
using SMAS.API.Services;
using System.Text;
using System.IO;
using System.Data;

// Load environment variables from .env file if it exists
var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
            continue;
            
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var value = parts[1].Trim();
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration (they'll override appsettings.json)
// Use the current directory (SMAS.API) as the base path so appsettings.json in the project is found.
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddEnvironmentVariables();

var inMemoryConfig = new Dictionary<string, string?>();
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")))
    inMemoryConfig["ConnectionStrings:DefaultConnection"] = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_KEY")))
    inMemoryConfig["Jwt:Key"] = Environment.GetEnvironmentVariable("JWT_KEY");
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_ISSUER")))
    inMemoryConfig["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER");
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_AUDIENCE")))
    inMemoryConfig["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")))
    inMemoryConfig["Jwt:ExpiryMinutes"] = Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES");
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_REFRESH_EXPIRY_DAYS")))
    inMemoryConfig["Jwt:RefreshExpiryDays"] = Environment.GetEnvironmentVariable("JWT_REFRESH_EXPIRY_DAYS");

if (inMemoryConfig.Count > 0)
{
    builder.Configuration.AddInMemoryCollection(inMemoryConfig);
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Serilog logging
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.Console();
    config.WriteTo.File("logs/smas-.txt", rollingInterval: RollingInterval.Day);
});

// Database with retry policy for resilience
builder.Services.AddDbContext<SmasDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions
            .EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null))
           .UseSnakeCaseNamingConvention());

// In-memory cache (no Redis dependency)
builder.Services.AddMemoryCache();

// Health checks
builder.Services.AddHealthChecks();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub/notifications"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Dependency Injection — Services
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IForecastService, ForecastService>();
builder.Services.AddScoped<IGeoAnalyticsService, GeoAnalyticsService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();
builder.Services.AddSingleton<IJobTrackingService, JobTrackingService>();

// Dependency Injection — Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ISaleRecordRepository, SaleRecordRepository>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// SignalR for realtime notifications
builder.Services.AddSignalR();

// Rate limiting on high-traffic endpoints
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SMAS API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] { }
    }});
});

// Background Service
builder.Services.AddHostedService<BackgroundJobService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serilog request logging
app.UseSerilogRequestLogging();

// Use built-in authentication middleware (must be before rate limiting for user-based partitioning)
app.UseAuthentication();
app.UseAuthorization();

// Rate limiting - must be after auth for user-based partitioning
app.UseRateLimiter();

// CORS must be before controllers
app.UseCors("AllowFrontend");

// Global exception handler
app.UseMiddleware<GlobalExceptionMiddleware>();

// Health checks
app.MapHealthChecks("/health");

// Root landing page
app.MapGet("/", () => Results.Ok("SMAS API is running successfully. Check status at /health."));

app.MapControllers();

// Map SignalR hubs
app.MapHub<SMAS.API.Hubs.NotificationHub>("/hub/notifications");

// Auto-migrate and seed
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SmasDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    // Log the connection string used for migrations (obscure password for security)
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
    logger.LogInformation("[DEBUG] Using DB connection string for migrations: {Conn}", connStr);
    
    try
    {
        Log.Information("Applying EF Core migrations...");
        dbContext.Database.Migrate();
        Log.Information("✓ Migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Migration failed, attempting EnsureCreated fallback");
        try
        {
            dbContext.Database.EnsureCreated();
            Log.Information("✓ Database created successfully (fallback)");
        }
        catch (Exception fallbackEx)
        {
            Log.Error(fallbackEx, "Failed to create database");
            throw;
        }
    }

    // Ensure legacy/new columns exist that may be missing from DB schema.
    try
    {
        var conn = dbContext.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            conn.Open();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='employees' AND column_name='monthly_salary'";
            var exists = cmd.ExecuteScalar();
            if (exists == null)
            {
                Log.Warning("Column 'monthly_salary' missing — adding it via ALTER TABLE");
                dbContext.Database.ExecuteSqlRaw("ALTER TABLE employees ADD COLUMN monthly_salary numeric(18,2) DEFAULT 0");
                Log.Information("✓ Added missing column 'monthly_salary'");
            }
        }
    }
    catch (Exception colEx)
    {
        Log.Warning(colEx, "Failed to verify or add 'monthly_salary' column; continuing to seeding");
    }

    // products.row_version is managed via EF Core migrations (AddRowVersionToProducts)

        try
        {
            Log.Information("Seeding initial data...");
            DbSeeder.Seed(dbContext);
            Log.Information("✓ Database seeded successfully");
        }
    catch (Exception seedEx)
    {
        Log.Error(seedEx, "Error during database seeding");
        throw;
    }
}

app.Run();