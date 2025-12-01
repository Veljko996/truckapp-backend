using MapsterMapper;
using Microsoft.EntityFrameworkCore;
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
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://gray-mushroom-0a8684603.3.azurestaticapps.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Set-Cookie");
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

// Authentication je obradjeno u JsonWebTokenMiddleware
// UseAuthorization() je zadržan za role-based authorization ako bude potrebno
builder.Services.AddAuthorization();

var app = builder.Build();


if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Production mode – Swagger exposed but no UI
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Truck API v1");
        c.DocumentTitle = "Truck API Docs";
    });
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<JsonWebTokenMiddleware>();

// UseAuthentication() je uklonjen jer JsonWebTokenMiddleware već postavlja context.User
// UseAuthorization() je zadržan za role-based authorization (npr. [Authorize(Roles = "Admin")])
app.UseAuthorization();

app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

app.Run();
