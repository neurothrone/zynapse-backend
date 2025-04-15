using Microsoft.EntityFrameworkCore;
using Zynapse.Backend.Persistence.Postgres.Data;
using Zynapse.Backend.Persistence.Postgres.Interfaces;
using Zynapse.Backend.Persistence.Postgres.Repositories;
using Zynapse.Backend.Persistence.Postgres.Utils;
using Zynapse.Backend.Api.Endpoints;
using Zynapse.Backend.Api.Services;
using Zynapse.Backend.Api.Extensions.Auth;
using Zynapse.Backend.Api.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Configure services
ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure middleware pipeline
ConfigureMiddlewarePipeline(app);

app.Run();

// Service configuration
void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
{
    // Database
    services.AddDbContext<ZynapseDbContext>(options =>
        options.UseNpgsql(
            configuration.GetConnectionString("SupabaseConnection") ??
            throw new InvalidOperationException("Connection string 'SupabaseConnection' not found.")
        )
    );

    // CORS
    services.AddCors(options =>
    {
        options.AddPolicy("AllowLocalhost3000", policyBuilder =>
        {
            policyBuilder.WithOrigins("http://localhost:3000") // Allow specific origin
                .AllowAnyMethod() // Allow all HTTP methods (GET, POST, etc.)
                .AllowAnyHeader()
                .AllowCredentials(); // Required for cookies, if used
        });
    });

    // Authentication & Authorization
    services.AddJwtAuthentication(configuration);
    services.AddAuthorization();

    // API Documentation
    services.AddEndpointsApiExplorer();
    services.AddSwaggerWithJwtAuth();

    // Application Services
    services.AddScoped<IJwtValidationService, JwtValidationService>();
    services.AddScoped<IProductRepository, ProductRepository>();
    services.AddScoped<IProductService, ProductService>();
}

// Middleware configuration
void ConfigureMiddlewarePipeline(WebApplication app)
{
    // Middleware execution order is important!
    
    // 1. CORS must be first
    app.UseCors("AllowLocalhost3000");

    // 2. Development-only middleware
    if (app.Environment.IsDevelopment())
    {
        // API documentation
        app.UseSwagger();
        app.UseSwaggerUI();
        
        // Middleware to provide detailed auth error information in development
        app.Use(async (context, next) =>
        {
            // Check and log authorization header format before proceeding
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                bool hasBearer = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
                bool looksLikeJwt = !hasBearer && authHeader.Contains('.') && authHeader.Split('.').Length == 3;
                
                app.Logger.LogDebug("Request Auth Header: {HasHeader}, Has Bearer Prefix: {HasBearer}, Looks Like JWT: {LooksLikeJwt}",
                    true, hasBearer, looksLikeJwt);
                
                if (!hasBearer && !looksLikeJwt)
                {
                    app.Logger.LogWarning("Authorization header present but does not follow Bearer scheme or JWT format");
                }
            }
            
            await next();
            
            if (context.Response.StatusCode == 401)
            {
                var authHeader = context.Request.Headers.Authorization.ToString();
                app.Logger.LogWarning(
                    "Authentication failed for {Path}. Auth header present: {HasHeader}. Starts with 'Bearer ': {HasBearer}",
                    context.Request.Path,
                    !string.IsNullOrEmpty(authHeader),
                    authHeader.StartsWith("Bearer ")
                );
            }
        });
    }

    // 3. Database seeding
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ZynapseDbContext>();
        ProductSeeder.SeedAsync(context, "Data/products.json").Wait();
    }

    // 4. Security middleware
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    // 5. Static files
    app.UseStaticFiles();
    
    // 6. Routing
    app.MapGet("/", context =>
    {
        context.Response.Redirect("/index.html");
        return Task.CompletedTask;
    });

    // 7. API endpoints
    app.MapProductEndpoints();
    app.MapAuthEndpoints();
}