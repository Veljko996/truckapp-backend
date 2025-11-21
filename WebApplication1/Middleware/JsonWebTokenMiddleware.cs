using Microsoft.AspNetCore.Authorization;
using WebApplication1.Utils;

namespace WebApplication1.Middleware;

public class JsonWebTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JsonWebTokenMiddleware> _logger;

    public JsonWebTokenMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<JsonWebTokenMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        // 1) [AllowAnonymous] -> pusti
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }

        // 2) Ako je [Authorize], validiraj JWT
        if (endpoint?.Metadata?.GetMetadata<IAuthorizeData>() != null)
        {
            // Read token from cookie (primary) or Authorization header (fallback)
            var token = CookieHelper.GetAccessToken(context.Request);
            
            if (string.IsNullOrWhiteSpace(token))
            {
                // Nema tokena – ovde možemo direktno da vratimo 401 (early exit)
                await WriteJsonError(context, StatusCodes.Status401Unauthorized,
                    "Nedostaje autentifikacioni token. Molimo prijavite se ponovo.");
                return;
            }
            var key = Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!);

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParams = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["AppSettings:Issuer"],
                    ValidAudience = _configuration["AppSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParams, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException("Neispravan algoritam potpisivanja tokena.");
                }

                // Postavi user-a u kontekst i nastavi
                context.User = principal;
                await _next(context);
                return;
            }
            catch (SecurityTokenExpiredException)
            {
                throw new UnauthorizedAccessException("Token je istekao. Pošaljite refresh token na /auth/refresh.");
            }
            catch (SecurityTokenException ex)
            {
                throw new UnauthorizedAccessException($"Nevažeći JWT token ({ex.Message}).");
            }
        }

        // 3) Ako nema ni [Authorize] ni [AllowAnonymous] – tretiraj kao public
        await _next(context);
    }

    private static async Task WriteJsonError(HttpContext context, int statusCode, string message)
    {
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = statusCode,
            message,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(payload));
    }
}
