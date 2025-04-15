using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Zynapse.Backend.Api.Services;

public interface IJwtValidationService
{
    bool ValidateToken(string token, out ClaimsPrincipal? claimsPrincipal);
    string? ExtractUserIdFromToken(string token);
    (bool isValid, string? message) AnalyzeToken(string token);
}

public class JwtValidationService : IJwtValidationService
{
    private readonly IConfiguration _configuration;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly ILogger<JwtValidationService> _logger;

    public JwtValidationService(IConfiguration configuration, ILogger<JwtValidationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var jwtSettings = configuration.GetSection("JWT");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        
        _tokenValidationParameters = new TokenValidationParameters
        {
            // For Supabase tokens
            ValidateIssuer = !string.IsNullOrEmpty(issuer),
            ValidIssuer = issuer,
            
            ValidateAudience = !string.IsNullOrEmpty(audience),
            ValidAudience = audience,
            
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.FromMinutes(5),
            
            RequireSignedTokens = true,
            RequireExpirationTime = true
        };
        
        _logger.LogInformation("JWT Validation Service initialized with: ValidateIssuer={ValidateIssuer}, " + 
                              "ValidateAudience={ValidateAudience}, " +
                              "Issuer={Issuer}, Audience={Audience}",
                              _tokenValidationParameters.ValidateIssuer, 
                              _tokenValidationParameters.ValidateAudience,
                              issuer, audience);
    }

    public bool ValidateToken(string token, out ClaimsPrincipal? claimsPrincipal)
    {
        claimsPrincipal = null;
        
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Token validation failed: Empty token");
            return false;
        }
            
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Strip 'Bearer ' prefix if present 
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length);
            }
            
            // Log token details before validation
            try 
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                _logger.LogInformation("Validating token: Issuer={Issuer}, Exp={Expiration}, Aud={Audience}", 
                    jwtToken.Issuer, 
                    jwtToken.ValidTo, 
                    string.Join(", ", jwtToken.Audiences));
            }
            catch 
            {
                // Just continue if we can't read the token for logging
            }
            
            claimsPrincipal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out SecurityToken validatedToken);
            _logger.LogInformation("Token validation succeeded");
            return true;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning("Token validation failed: Token expired at {Expiry}", ex.Expires);
            return false;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            _logger.LogError("Token validation failed: Invalid signature. Check if the JWT secret matches Supabase's");
            return false;
        }
        catch (SecurityTokenInvalidIssuerException ex)
        {
            _logger.LogError("Token validation failed: Invalid issuer {Issuer}, expected {ExpectedIssuer}", 
                ex.InvalidIssuer, _tokenValidationParameters.ValidIssuer);
            return false;
        }
        catch (SecurityTokenInvalidAudienceException ex)
        {
            _logger.LogError("Token validation failed: Invalid audience {Audience}, expected {ExpectedAudience}", 
                ex.InvalidAudience, _tokenValidationParameters.ValidAudience);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed with exception: {Message}", ex.Message);
            return false;
        }
    }

    public string? ExtractUserIdFromToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Cannot extract user ID: Token is empty");
            return null;
        }
            
        try
        {
            // Strip 'Bearer ' prefix if present 
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length);
            }
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Log all claims for debugging
            foreach (var claim in jwtToken.Claims)
            {
                _logger.LogDebug("Token claim: {Type} = {Value}", claim.Type, claim.Value);
            }
            
            // For Supabase, check multiple possible ID claim types in order of preference
            var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
            if (userId == null)
            {
                userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            }
            if (userId == null)
            {
                userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            }
            
            if (userId == null)
            {
                _logger.LogWarning("No user ID found in token claims. Available claims: {Claims}", 
                    string.Join(", ", jwtToken.Claims.Select(c => c.Type)));
            }
            else
            {
                _logger.LogInformation("Successfully extracted user ID: {UserId}", userId);
            }
            
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract user ID from token: {Message}", ex.Message);
            return null;
        }
    }
    
    public (bool isValid, string? message) AnalyzeToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return (false, "Token is empty");
        }
            
        try
        {
            // Strip 'Bearer ' prefix if present 
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length);
            }
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Log token information
            var jwtSettings = _configuration.GetSection("JWT");
            var configuredIssuer = jwtSettings["Issuer"];
            var configuredAudience = jwtSettings["Audience"];
            
            _logger.LogInformation(
                "Token Analysis: Issuer={TokenIssuer}, ConfiguredIssuer={ConfiguredIssuer}, " +
                "Audience={TokenAudience}, ConfiguredAudience={ConfiguredAudience}, " +
                "Expiry={TokenExpiry}",
                jwtToken.Issuer, configuredIssuer,
                string.Join(",", jwtToken.Audiences), configuredAudience,
                jwtToken.ValidTo);
            
            // Check if token has expired
            var now = DateTime.UtcNow;
            if (jwtToken.ValidTo < now)
            {
                return (false, $"Token expired on {jwtToken.ValidTo}");
            }
            
            // Check issuer if validation is enabled
            if (_tokenValidationParameters.ValidateIssuer && 
                !string.IsNullOrEmpty(_tokenValidationParameters.ValidIssuer) &&
                !string.Equals(jwtToken.Issuer, _tokenValidationParameters.ValidIssuer) &&
                !jwtToken.Issuer.StartsWith(_tokenValidationParameters.ValidIssuer))
            {
                return (false, $"Invalid issuer. Expected: {_tokenValidationParameters.ValidIssuer}, Got: {jwtToken.Issuer}");
            }
            
            // Check audience if validation is enabled
            if (_tokenValidationParameters.ValidateAudience &&
                !string.IsNullOrEmpty(_tokenValidationParameters.ValidAudience) &&
                !jwtToken.Audiences.Contains(_tokenValidationParameters.ValidAudience))
            {
                return (false, $"Invalid audience. Expected: {_tokenValidationParameters.ValidAudience}, Got: {string.Join(",", jwtToken.Audiences)}");
            }
            
            // Attempt signature validation
            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
                
                // Output claims structure
                var claims = jwtToken.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                _logger.LogInformation("Token structure: Issuer: {Issuer}, Subject: {Subject}, Claims count: {ClaimsCount}", 
                    jwtToken.Issuer, jwtToken.Subject, claims.Count);
                
                return (true, "Token is valid");
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
                _logger.LogError("Token signature key not found");
                return (false, "Token signature key not found. Check if the secret matches the one used by Supabase");
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogError("Token signature is invalid");
                return (false, "Token signature is invalid. The token may have been tampered with or the secret is incorrect");
            }
            catch (SecurityTokenInvalidIssuerException ex)
            {
                return (false, $"Invalid issuer: {ex.InvalidIssuer}");
            }
            catch (SecurityTokenInvalidAudienceException ex)
            {
                return (false, $"Invalid audience: {ex.InvalidAudience}");
            }
            catch (Exception validationEx)
            {
                _logger.LogError(validationEx, "Token validation error");
                return (false, $"Token validation error: {validationEx.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token parsing error");
            return (false, $"Token parsing error: {ex.Message}");
        }
    }
} 