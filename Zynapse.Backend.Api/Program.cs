using Microsoft.EntityFrameworkCore;
using Zynapse.Backend.Persistence.Postgres.Data;
using Zynapse.Backend.Persistence.Postgres.Interfaces;
using Zynapse.Backend.Persistence.Postgres.Repositories;
using Zynapse.Backend.Persistence.Postgres.Utils;
using Zynapse.Backend.Api.Endpoints.Auth;
using Zynapse.Backend.Api.Endpoints.Cart;
using Zynapse.Backend.Api.Endpoints.Product;
using Zynapse.Backend.Api.Services;
using Zynapse.Backend.Api.Services.Cart;
using Zynapse.Backend.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration, builder.Environment);
var app = builder.Build();
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
    services.AddScoped<ICartRepository, CartRepository>();
    services.AddScoped<ICartService, CartService>();
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
    app.MapAuthEndpoints();
    app.MapCartEndpoints();
    app.MapProductEndpoints();
}