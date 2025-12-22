using WebApplication1.Services.AuthenticationServices;
using WebApplication1.Services.DashboardServices;
using WebApplication1.Services.NasaVozilaServices;
using WebApplication1.Services.PoslovnicaServices;
using WebApplication1.Services.PrevozniciServices;
using WebApplication1.Services.TuraServices;
using WebApplication1.Services.UserServices;
using WebApplication1.Services.VinjeteServices;
using WebApplication1.Services.VrstaNadogradnjeServices;
using WebApplication1.Services.KlijentServices;
using WebApplication1.Repository.DashboardRepository;
using WebApplication1.Repository.PoslovnicaRepository;
using WebApplication1.Repository.PrevozniciRepository;
using WebApplication1.Repository.UserRepository;
using WebApplication1.Repository.NalogRepository;
using WebApplication1.Repository.VrstaNadogradnjeRepository;
using WebApplication1.Repository.KlijentRepository;
using WebApplication1.Services.NalogServices;

namespace WebApplication1.Configuration;

public static class ApiConfig 
{
    public static WebApplicationBuilder AddApiConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<INasaVozilaService,NasaVozilaService>();
        builder.Services.AddScoped<INasaVozilaRepository, NasaVozilaRepository>();

        builder.Services.AddScoped<IVinjeteService, VinjeteService>();
        builder.Services.AddScoped<IVinjeteRepository, VinjeteRepository>();
        
        builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        
        builder.Services.AddScoped<ILoggingRepository, LoggingRepository>();
        builder.Services.AddScoped<ILogService, LogService>();
        
        builder.Services.AddScoped<ITureRepository, TureRepository>();
        builder.Services.AddScoped<ITuraService, TuraService>();
        
        // User and Employee services
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddScoped<INalogRepository, NalogRepository>();
        builder.Services.AddScoped<INalogService, NalogService>();

        // Poslovnica services
        builder.Services.AddScoped<IPoslovnicaRepository, PoslovnicaRepository>();
        builder.Services.AddScoped<IPoslovnicaService, PoslovnicaService>();
        
        // Prevoznici services
        builder.Services.AddScoped<IPrevozniciRepository, PrevozniciRepository>();
        builder.Services.AddScoped<IPrevozniciService, PrevozniciService>();
        
        // VrstaNadogradnje services
        builder.Services.AddScoped<IVrstaNadogradnjeRepository, VrstaNadogradnjeRepository>();
        builder.Services.AddScoped<IVrstaNadogradnjeService, VrstaNadogradnjeService>();
        
        // Klijent services
        builder.Services.AddScoped<IKlijentRepository, KlijentRepository>();
        builder.Services.AddScoped<IKlijentService, KlijentService>();
        
        // Dashboard services
        builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();

        return builder;
    }

}
