using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace WebApplication1.Utils.Jwt;

public class ManualJwtHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public ManualJwtHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Ako je korisnik već autentifikovan (postavljen u context.User)
        if (Context.User?.Identity?.IsAuthenticated == true)
        {
            var ticket = new AuthenticationTicket(Context.User, "ManualJwtScheme");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        // Ako nema tokena ne failuj, samo pusti pipeline da nastavi
        return Task.FromResult(AuthenticateResult.NoResult());
    }

}
