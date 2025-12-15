namespace WebApplication1.Utils.Jwt;

public static class JwtHelperPrincipal
{
    public static ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token ne može biti null ili prazan.", nameof(token));

        var key = Encoding.UTF8.GetBytes(configuration["AppSettings:Token"]!);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = false // dozvoli istekli token
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        ClaimsPrincipal principal;
        
        try
        {
            principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token algorithm");
            }
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException($"Token validacija neuspešna: {ex.Message}", ex);
        }

        return principal;
    }
}
