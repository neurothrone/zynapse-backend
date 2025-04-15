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
        var isDebugMode = configuration.GetValue<bool>("DebugAuth", false);

        if (string.IsNullOrEmpty(secret) && !isDebugMode)
        {
            throw new InvalidOperationException("JWT Secret is not configured");
        }

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
                    ValidateIssuer = !isDebugMode,
                    ValidateAudience = !isDebugMode,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = !isDebugMode,
                    IssuerSigningKey = !string.IsNullOrEmpty(secret)
                        ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                        : null,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                if (!isDebugMode && !string.IsNullOrEmpty(jwtSettings["Issuer"]))
                {
                    options.TokenValidationParameters.ValidIssuer = jwtSettings["Issuer"];
                }

                if (!isDebugMode && !string.IsNullOrEmpty(jwtSettings["Audience"]))
                {
                    options.TokenValidationParameters.ValidAudience = jwtSettings["Audience"];
                }

                // Configure HTTPS requirements
                options.RequireHttpsMetadata = !configuration.GetValue<bool>("DisableHttpsRequirement", false);
                options.SaveToken = true;

                // Add event handlers for debugging and debugging bypass
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogError("Authentication failed: {Exception}", context.Exception);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogInformation("Token validated successfully for user: {UserId}",
                            context.Principal?.FindFirst("sub")?.Value);
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}