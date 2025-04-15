using System.IdentityModel.Tokens.Jwt;
using Zynapse.Backend.Api.Services;

namespace Zynapse.Backend.Api.Endpoints;

/// <summary>
/// Endpoint mapping for authentication-related API routes
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/v1/auth");

        // Authentication endpoints
        group.MapGet("validate", AuthHandlers.ValidateTokenAsync)
            .WithSummary("Validate the authentication token.")
            .RequireAuthorization();

        // Token test endpoint (public)
        group.MapPost("test-token", AuthHandlers.TestTokenAsync)
            .WithSummary("Test a JWT token.");

        // Configuration endpoint (public)
        group.MapGet("config", AuthHandlers.GetAuthConfigAsync)
            .WithSummary("Get JWT configuration details.");
            
        // Note: Token generation is handled by Supabase Auth directly.
        // The frontend should authenticate with Supabase, and this API 
        // only validates the tokens issued by Supabase.
    }
}

/// <summary>
/// Handlers for authentication-related endpoints
/// </summary>
public static class AuthHandlers
{
    /// <summary>
    /// Validates the user's authentication token
    /// </summary>
    public static IResult ValidateTokenAsync(HttpContext httpContext)
    {
        // This endpoint is protected, so if we get here, the user is authenticated
        var userId = httpContext.User.FindFirst("sub")?.Value;
        
        return TypedResults.Ok(new
        {
            IsAuthenticated = true,
            UserId = userId
        });
    }
    
    /// <summary>
    /// Test a JWT token for troubleshooting
    /// </summary>
    public static IResult TestTokenAsync(IJwtValidationService jwtService, string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return TypedResults.BadRequest(new { Error = "Token is required" });
        }
            
        // Strip 'Bearer ' prefix if present 
        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring("Bearer ".Length);
        }
            
        try
        {
            // Analyze the token
            var (isValid, message) = jwtService.AnalyzeToken(token);
            
            // Extract user ID regardless of validity
            var userId = jwtService.ExtractUserIdFromToken(token);
            
            // Parse the token to get claims and basic info
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            return TypedResults.Ok(new
            {
                IsValid = isValid,
                Message = message,
                UserId = userId,
                TokenInfo = new 
                {
                    Issuer = jwtToken.Issuer,
                    Audience = string.Join(", ", jwtToken.Audiences),
                    ValidFrom = jwtToken.ValidFrom,
                    ValidTo = jwtToken.ValidTo,
                    IsExpired = DateTime.UtcNow > jwtToken.ValidTo
                }
            });
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new { Error = $"Failed to parse token: {ex.Message}" });
        }
    }
    
    /// <summary>
    /// Returns the current JWT authentication configuration
    /// </summary>
    public static IResult GetAuthConfigAsync(IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JWT");
        
        return TypedResults.Ok(new
        {
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            HasSecret = !string.IsNullOrEmpty(jwtSettings["Secret"]),
            ExpiryInMinutes = jwtSettings["ExpiryInMinutes"]
        });
    }
}