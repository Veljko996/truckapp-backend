using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebApplication1.Configuration;
using WebApplication1.Middleware;
using WebApplication1.Utils.Mapping;

var builder = WebApplication.CreateBuilder(args);

// ================= DB =================
builder.Services.AddDbContext<TruckContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ================= CORS =================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = new[]
        {
            "http://localhost:5173",
            "https://gray-mushroom-0a8684603.3.azurestaticapps.net"
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

// ================= AUTH (JWT + Cookies) =================
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
            ClockSkew = TimeSpan.Zero
        };

        // 🔑 Čitanje JWT-a iz HttpOnly cookie-ja
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["accessToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ================= MAPSTER =================
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

var app = builder.Build();

// ================= SWAGGER =================
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

// ================= PIPELINE =================
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseAuthentication();   
app.UseAuthorization();

app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

app.Run();
