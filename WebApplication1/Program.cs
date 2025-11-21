using MapsterMapper;
using Microsoft.AspNetCore.Authentication;
using WebApplication1.Configuration;
using WebApplication1.Middleware;
using WebApplication1.Utils.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TruckContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(TruckContext)))
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var config = TypeAdapterConfig.GlobalSettings;
MappingConfig.RegisterMappings();

config.Default.IgnoreNullValues(true);
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

builder.Services.AddControllers();
builder.AddApiConfiguration();
builder.Services.AddSwaggerConfiguration();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication("ManualJwtScheme")
    .AddScheme<AuthenticationSchemeOptions, ManualJwtHandler>("ManualJwtScheme", null);

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Truck API v1");
});

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

// ErrorHandlerMiddleware MUST be first to catch all exceptions
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseMiddleware<JsonWebTokenMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// RequestLoggingMiddleware should be after ErrorHandler to log responses
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

app.Run();
