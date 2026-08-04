using System.Threading.RateLimiting;
using MapsterMapper;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using QuestPDF.Infrastructure;
using WebApplication1.Configuration;
using WebApplication1.Middleware;
using WebApplication1.Utils.Mapping;

QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.EnableDebugging = false;

var builder = WebApplication.CreateBuilder(args);

// ================= DB =================
builder.Services.AddDbContext<TruckContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TruckContext"))
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = new[]
        {
            "http://localhost:5173",
            "https://gray-mushroom-0a8684603.3.azurestaticapps.net",
            "https://nalogflow.rs",
			"https://www.nalogflow.rs"

		};

        var additionalOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        if (additionalOrigins != null && additionalOrigins.Length > 0)
        {
            allowedOrigins = allowedOrigins.Concat(additionalOrigins).ToArray();
        }

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)
            ),
        };

        //  Čitanje JWT-a iz HttpOnly cookie-ja
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Za refresh endpoint, ne čitaj token - dozvoli da prođe bez autentifikacije
                if (context.Request.Path.StartsWithSegments("/api/auth/refresh-token"))
                {
                    return Task.CompletedTask;
                }

                var token = context.Request.Cookies["accessToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                // Za refresh endpoint, ne vraćaj grešku - dozvoli da se pozove
                if (context.Request.Path.StartsWithSegments("/api/auth/refresh-token"))
                {
                    context.NoResult();
                    return Task.CompletedTask;
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // Za refresh endpoint, ne vraćaj 401 challenge
                if (context.Request.Path.StartsWithSegments("/api/auth/refresh-token"))
                {
                    context.HandleResponse();
                    return Task.CompletedTask;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();


var config = TypeAdapterConfig.GlobalSettings;
MappingConfig.RegisterMappings();
config.Default.IgnoreNullValues(true);

builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// ================= SERVICES =================
builder.Services.AddControllers();
builder.AddApiConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationInsightsTelemetry();

// ================= EXCEPTION HANDLING =================
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ================= RATE LIMITING =================
// Štiti login od brute-force napada na lozinke. Primenjuje se SAMO na login
// (refresh token je 256-bitni random pa je brute-force nemoguć; rate limit tamo
// samo nosi rizik lažnog zaključavanja iza deljenog kancelarijskog NAT IP-a).
//
// Klijentski IP: Azure App Service front-end DODAJE stvarni klijentski IP kao POSLEDNJI unos
// u X-Forwarded-For (napadač može da "prepend"-uje lažne IP-jeve levo, ali ne posle Azure unosa).
// Zato uzimamo POSLEDNJI unos (ne prvi) i skidamo :port (ephemeral, menja se po konekciji) da
// dobijemo stabilan ključ koji se ne može trivijalno lažirati. Fallback na RemoteIpAddress lokalno.
// Namerno NE koristimo ForwardedHeaders middleware: sa očišćenim KnownProxies header se ne bi
// obrađivao i svi bi pali u jednu particiju (globalno zaključavanje).
static string ResolveClientIp(HttpContext ctx)
{
    var xff = ctx.Request.Headers["X-Forwarded-For"].ToString();
    if (!string.IsNullOrWhiteSpace(xff))
    {
        var last = xff.Split(',')[^1].Trim();
        // Skini :port kod IPv4 "a.b.c.d:port" (ephemeral port bi napravio novu particiju po zahtevu).
        var colon = last.LastIndexOf(':');
        if (colon > 0 && last.IndexOf('.') > 0)
            last = last[..colon];
        if (!string.IsNullOrWhiteSpace(last))
            return last;
    }
    return ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext =>
    {
        var clientIp = ResolveClientIp(httpContext);

        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });

    // Kad limiter odbije zahtev, ASP.NET podrazumevano vraća PRAZAN body -> frontend prikaže prazan
    // toast. Ovde vraćamo lokalizovani JSON (isti oblik kao GlobalExceptionHandler: camelCase "message")
    // + Retry-After header da klijent zna koliko da čeka.
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = "60";
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"status\":429,\"type\":\"RateLimited\",\"message\":\"Previše pokušaja prijave. Sačekajte minut i pokušajte ponovo.\"}",
            cancellationToken);
    };
});


var app = builder.Build();

// ================= SWAGGER =================
// Swagger je izložen SAMO u Development/Staging. U Production je isključen da API šema
// (svi endpointi, modeli) ne bude javno dostupna.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ================= PIPELINE =================
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowFrontend");

// Modern exception handling (replaces ErrorHandlerMiddleware)
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

app.Run();
