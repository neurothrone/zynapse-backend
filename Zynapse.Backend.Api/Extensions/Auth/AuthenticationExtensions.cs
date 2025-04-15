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
            // Configure token validation for Supabase tokens
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // For Supabase, we should validate the issuer and audience if provided
                ValidateIssuer = !string.IsNullOrEmpty(issuer),
                ValidIssuer = issuer,
                
                ValidateAudience = !string.IsNullOrEmpty(audience),
                ValidAudience = audience,
                
                ValidateLifetime = true,     // Always validate token expiration
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ClockSkew = TimeSpan.FromMinutes(5),  // Allow for some clock skew
                
                // Use a single audience string or a list as needed
                ValidateActor = false,
                ValidTypes = new[] { "JWT" },
                RequireSignedTokens = true,
                RequireExpirationTime = true
            };

            // Configure security requirements
            options.RequireHttpsMetadata = !configuration.GetValue<bool>("DisableHttpsRequirement", false);
            options.SaveToken = true;
            
            // Add middleware to parse Authorization header
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    
                    // First, check if we have a token already set (could be from a different source like query param)
                    if (string.IsNullOrEmpty(context.Token) && context.Request.Headers.ContainsKey("Authorization"))
                    {
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        
                        // If it starts with "Bearer ", extract the token
                        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authHeader.Substring("Bearer ".Length).Trim();
                            logger.LogInformation("Extracted token from Authorization header with Bearer prefix");
                        }
                        // If no "Bearer " prefix but still looks like a JWT, use it directly
                        else if (!string.IsNullOrEmpty(authHeader) && 
                                 (authHeader.Contains('.') && authHeader.Split('.').Length == 3))
                        {
                            context.Token = authHeader.Trim();
                            logger.LogInformation("Using token from Authorization header without Bearer prefix");
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(context.Token) && context.Token.Length > 10)
                    {
                        logger.LogDebug("Token received for authentication (first 10 chars): {TokenStart}...", 
                            context.Token.Substring(0, 10));
                    }
                    else
                    {
                        logger.LogWarning("No valid token found in request");
                    }
                    
                    return Task.CompletedTask;
                },
                
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        logger.LogWarning("Token has expired");
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    else
                    {
                        logger.LogError("Authentication failed: {ExceptionType}: {Exception}", 
                            context.Exception.GetType().Name, context.Exception.Message);
                    }
                    
                    return Task.CompletedTask;
                },
                
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    var userId = context.Principal?.FindFirst("sub")?.Value ?? "unknown";
                    logger.LogInformation("Token validated successfully for user: {UserId}", userId);
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}