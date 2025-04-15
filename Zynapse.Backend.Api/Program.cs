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
        
        // Debug authentication bypass
        bool isDebugMode = app.Configuration.GetValue<bool>("DebugAuth", false);
        if (isDebugMode)
        {
            app.Use(async (context, next) =>
            {
                string authHeader = context.Request.Headers.Authorization.ToString();
                
                // Only apply to endpoints with [Authorize] attribute
                var endpoint = context.GetEndpoint();
                if (endpoint?.Metadata.GetMetadata<AuthorizeAttribute>() != null && !string.IsNullOrEmpty(authHeader))
                {
                    // Create debug identity for any request with an Authorization header in debug mode
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "DebugUser"),
                        new Claim(ClaimTypes.NameIdentifier, "debug-user-id"),
                        new Claim("sub", "debug-user-id")
                    };
                    
                    var identity = new ClaimsIdentity(claims, "Debug", ClaimTypes.Name, ClaimTypes.Role);
                    context.User = new ClaimsPrincipal(identity);
                    
                    app.Logger.LogWarning("DEBUG MODE: Authentication bypassed for {Path}", context.Request.Path);
                }
                
                await next();
            });
        }
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