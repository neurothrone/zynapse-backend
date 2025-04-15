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
            .WithDescription("Returns the user's authentication status and ID if authenticated.")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        // Public info endpoint
        group.MapGet("info", AuthHandlers.GetAuthInfoAsync)
            .WithSummary("Get authentication information.")
            .WithDescription("Returns information about which endpoints require authentication.")
            .Produces<object>(StatusCodes.Status200OK);
            
        // Token test endpoint (public)
        group.MapPost("test-token", AuthHandlers.TestTokenAsync)
            .WithSummary("Test a JWT token.")
            .WithDescription("Analyzes a JWT token for troubleshooting purposes - does not require authentication.")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
            
        // Configuration test endpoint (public)
        group.MapGet("config", AuthHandlers.GetAuthConfigAsync)
            .WithSummary("Get JWT configuration details.")
            .WithDescription("Returns information about how JWT authentication is configured.")
            .Produces<object>(StatusCodes.Status200OK);
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
        var claims = httpContext.User.Claims.Select(c => new { c.Type, c.Value }).ToList();

        return TypedResults.Ok(new
        {
            IsAuthenticated = true,
            UserId = userId,
            Message = "Token is valid",
            AuthenticationType = httpContext.User.Identity?.AuthenticationType,
            Claims = claims
        });
    }

    /// <summary>
    /// Returns information about authentication requirements
    /// </summary>
    public static IResult GetAuthInfoAsync()
    {
        return TypedResults.Ok(new
        {
            Message = "Authentication is required for modifying resources",
            PublicEndpoints = new[]
            {
                "GET /api/v1/products",
                "GET /api/v1/products/{id}",
                "GET /api/v1/products/random",
                "GET /api/v1/products/categories"
            },
            ProtectedEndpoints = new[]
            {
                "POST /api/v1/products",
                "PUT /api/v1/products/{id}",
                "DELETE /api/v1/products/{id}"
            }
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
            
            // Try to parse the token to get claims
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            var claims = jwtToken.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            return TypedResults.Ok(new
            {
                IsValid = isValid,
                Message = message,
                UserId = userId,
                TokenInfo = new 
                {
                    Issuer = jwtToken.Issuer,
                    Subject = jwtToken.Subject,
                    Audience = string.Join(", ", jwtToken.Audiences),
                    ValidFrom = jwtToken.ValidFrom,
                    ValidTo = jwtToken.ValidTo,
                    IsExpired = DateTime.UtcNow > jwtToken.ValidTo
                },
                Claims = claims
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
            JWT = new
            {
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                // Don't expose the actual secret, just if it's configured
                HasSecret = !string.IsNullOrEmpty(jwtSettings["Secret"]),
                SecretLength = jwtSettings["Secret"]?.Length ?? 0,
                ExpiryInMinutes = jwtSettings["ExpiryInMinutes"]
            },
            Usage = new
            {
                SwaggerUsage = "Click 'Authorize' and enter: Bearer your_token_here",
                CurlExample = "curl -X 'POST' 'https://localhost:7001/api/v1/products' -H 'Authorization: Bearer your_token_here' -H 'Content-Type: application/json' -d '{...}'",
                TestEndpoint = "POST to /api/v1/auth/test-token with your token to diagnose issues"
            }
        });
    }
}