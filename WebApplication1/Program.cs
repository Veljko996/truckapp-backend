using MapsterMapper;
using System.Linq;
using WebApplication1.Configuration;
using WebApplication1.Middleware;
using WebApplication1.Utils.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TruckContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Dodaj sve frontend origin-e (development i production)
        var allowedOrigins = new[]
        {
            "http://localhost:5173",
            "https://gray-mushroom-0a8684603.3.azurestaticapps.net"
        };

        // Ako imaš custom domen za frontend, dodaj ga ovde
        var additionalOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (additionalOrigins != null && additionalOrigins.Length > 0)
        {
            allowedOrigins = allowedOrigins.Concat(additionalOrigins).ToArray();
        }

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials() // Obavezno za cookies
            .WithExposedHeaders("Set-Cookie", "Access-Control-Allow-Credentials")
            .SetPreflightMaxAge(TimeSpan.FromHours(24)); // Cache preflight za 24h
    });
});

//  MAPSTER
var config = TypeAdapterConfig.GlobalSettings;
MappingConfig.RegisterMappings();
config.Default.IgnoreNullValues(true);

builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

//  SERVICES
builder.Services.AddControllers();
builder.AddApiConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();


builder.Services.AddAuthorization();

var app = builder.Build();


if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Truck API v1");
        c.DocumentTitle = "Truck API Docs";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<JsonWebTokenMiddleware>();

app.UseAuthorization();

app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

app.Run();
