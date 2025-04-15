using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Zynapse.Backend.Api.Extensions.Auth;

/// <summary>
/// Extensions for configuring JWT authentication in the application
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds JWT authentication to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JWT");
        var secret = jwtSettings["Secret"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException("JWT Secret is not configured");

        if (string.IsNullOrEmpty(issuer))
            throw new InvalidOperationException("JWT Issuer is not configured");

        if (string.IsNullOrEmpty(audience))
            throw new InvalidOperationException("JWT Audience is not configured");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Configure token validation parameters
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = true,
                    ValidAudience = audience,

                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                options.SaveToken = true;

                // Handle authorization header extraction
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].ToString();

                        if (string.IsNullOrEmpty(authHeader))
                            return Task.CompletedTask;

                        // Extract token with or without Bearer prefix
                        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authHeader.Substring("Bearer ".Length).Trim();
                        }
                        else if (authHeader.Contains('.') && authHeader.Split('.').Length == 3)
                        {
                            // Looks like a raw JWT token
                            context.Token = authHeader.Trim();
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}