using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Zynapse.Backend.Api.Services;

/// <summary>
/// Service for validating JWT tokens issued by Supabase Auth.
/// This service does not generate tokens - token generation is handled by Supabase.
/// </summary>
public interface IJwtValidationService
{
    (bool isValid, string message) AnalyzeToken(string token);
    string? ExtractUserIdFromToken(string token);
}

public class JwtValidationService : IJwtValidationService
{
    private readonly IConfiguration _configuration;

    public JwtValidationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (bool isValid, string message) AnalyzeToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var tokenIssuer = jwtToken.Issuer;
            var configIssuer = _configuration["JWT:Issuer"];

            // Check if the issuer matches or if the token issuer starts with the configured issuer
            if (tokenIssuer != configIssuer && !tokenIssuer.StartsWith(configIssuer!))
            {
                return (false, $"Invalid issuer. Expected: {configIssuer}, Actual: {tokenIssuer}");
            }

            // Check audience
            var tokenAudience = jwtToken.Audiences.FirstOrDefault();
            var configAudience = _configuration["JWT:Audience"];

            if (tokenAudience != configAudience)
            {
                return (false, $"Invalid audience. Expected: {configAudience}, Actual: {tokenAudience}");
            }

            // Check expiration
            if (DateTime.UtcNow > jwtToken.ValidTo)
            {
                return (false, $"Token expired at {jwtToken.ValidTo}");
            }

            return (true, "Token is valid");
        }
        catch (Exception)
        {
            return (false, "Failed to validate token");
        }
    }

    public string? ExtractUserIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Try to extract user ID from standard claims
            string? userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            }

            return userId;
        }
        catch
        {
            return null;
        }
    }
}