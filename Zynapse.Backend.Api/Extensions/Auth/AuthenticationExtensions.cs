using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Zynapse.Backend.Api.Extensions.Auth;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
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
            // Relaxed validation for debugging
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = !isDebugMode,
                ValidateIssuerSigningKey = !isDebugMode,
                IssuerSigningKey = !string.IsNullOrEmpty(secret) ? 
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)) : null,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            // Set these to true when using HTTPS
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            
            // Add events for debugging and potentially bypass validation in debug mode
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogError("Authentication failed: {Exception}", context.Exception);
                    
                    // If in debug mode, log but don't fail authentication
                    if (isDebugMode)
                    {
                        // Create a debug authentication ticket
                        var identity = new ClaimsIdentity(new[] { 
                            new Claim(ClaimTypes.Name, "DebugUser"),
                            new Claim(ClaimTypes.NameIdentifier, "debug-user-id"),
                            new Claim("sub", "debug-user-id") 
                        }, JwtBearerDefaults.AuthenticationScheme);
                        
                        var principal = new ClaimsPrincipal(identity);
                        var ticket = new AuthenticationTicket(principal, JwtBearerDefaults.AuthenticationScheme);
                        
                        context.NoResult();
                        context.Response.StatusCode = 200;
                        context.Success();
                        context.Principal = principal;
                    }
                    
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogInformation("Token validated successfully");
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogInformation("Token received: {Token}", 
                        context.Token?.Length > 10 ? 
                            $"{context.Token.Substring(0, 10)}..." : context.Token);
                            
                    // In debug mode, accept any token
                    if (isDebugMode && !string.IsNullOrEmpty(context.Token))
                    {
                        // Create a debug authentication ticket
                        var identity = new ClaimsIdentity(new[] { 
                            new Claim(ClaimTypes.Name, "DebugUser"),
                            new Claim(ClaimTypes.NameIdentifier, "debug-user-id"),
                            new Claim("sub", "debug-user-id") 
                        }, JwtBearerDefaults.AuthenticationScheme);
                        
                        var principal = new ClaimsPrincipal(identity);
                        context.Principal = principal;
                        context.Success();
                    }
                    
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
} 