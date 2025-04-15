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
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ZynapseDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("SupabaseConnection") ??
        throw new InvalidOperationException("Connection string 'SupabaseConnection' not found.")
    )
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:3000") // Allow specific origin
            .AllowAnyMethod() // Allow all HTTP methods (GET, POST, etc.)
            .AllowAnyHeader()
            .AllowCredentials(); // Required for cookies, if used
    });
});

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// Register JWT validation service
builder.Services.AddScoped<IJwtValidationService, JwtValidationService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwtAuth(); // Use the custom Swagger extension

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

app.UseCors("AllowLocalhost3000");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Add development bypass for authentication (complete override)
    bool isDebugMode = app.Configuration.GetValue<bool>("DebugAuth", false);
    if (isDebugMode)
    {
        // BYPASS: Authentication in development mode
        app.Use(async (context, next) =>
        {
            // Log all requests for debugging
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
            
            // Check if this is a protected endpoint that needs auth
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<AuthorizeAttribute>() != null)
            {
                string authHeader = context.Request.Headers.Authorization.ToString();
                logger.LogWarning("Protected endpoint accessed: {Path}, Auth Header: {HasAuth}", 
                    context.Request.Path, !string.IsNullOrEmpty(authHeader));
                
                // Create a debug identity - simulate a logged in user
                // This happens regardless of the token validity since we're in debug mode
                if (!string.IsNullOrEmpty(authHeader))
                {
                    var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, "DebugUser"),
                        new Claim(ClaimTypes.NameIdentifier, "debug-user-id"),
                        new Claim("sub", "debug-user-id")
                    }, "Debug", ClaimTypes.Name, ClaimTypes.Role);
                    
                    context.User = new ClaimsPrincipal(identity);
                    
                    // Mark the user as authenticated
                    await context.AuthenticateAsync();
                    
                    logger.LogWarning("DEBUG MODE: Authentication bypassed for {Path}", context.Request.Path);
                }
            }
            
            await next();
        });
    }
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ZynapseDbContext>();
    await ProductSeeder.SeedAsync(context, "Data/products.json");
}

app.UseHttpsRedirection();

// Add Authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapGet("/", context =>
{
    context.Response.Redirect("/index.html");
    return Task.CompletedTask;
});

app.MapProductEndpoints();
app.MapAuthEndpoints();

app.Run();