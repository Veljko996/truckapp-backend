using WebApplication1.Services.AuthenticationServices;
using WebApplication1.Services.DashboardServices;
using WebApplication1.Services.ExceptionServices;
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
using WebApplication1.Services.NalogTroskoviServices;
using WebApplication1.Repository.NalogTroskoviRepository;
using WebApplication1.Services.NalogPrihodiServices;
using WebApplication1.Repository.NalogPrihodiRepository;
using WebApplication1.Services.NalogDokumentiServices;
using WebApplication1.Repository.NalogDokumentiRepository;
using WebApplication1.Services.FileStorage;
using WebApplication1.Services.QuestPdfServices;
using WebApplication1.Services.GorivoServices;
using WebApplication1.Repository.GorivoRepository;
using WebApplication1.Services.EmployeeServices;
using WebApplication1.Repository.EmployeeRepository;
using WebApplication1.Services.DriverAssignmentServices;
using WebApplication1.Repository.DriverAssignmentRepository;
using WebApplication1.Services.NalogVozacAccessServices;
using WebApplication1.Services.QueuePublisherServices;
using WebApplication1.Utils.Tenant;

namespace WebApplication1.Configuration;

public static class ApiConfig 
{
    public static WebApplicationBuilder AddApiConfiguration(this WebApplicationBuilder builder)
    {
        // Tenant
        builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();

        builder.Services.AddScoped<INasaVozilaService,NasaVozilaService>();
        builder.Services.AddScoped<INasaVozilaRepository, NasaVozilaRepository>();

        builder.Services.AddScoped<IVinjeteService, VinjeteService>();
        builder.Services.AddScoped<IVinjeteRepository, VinjeteRepository>();
        
        builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        
        builder.Services.AddScoped<ILoggingRepository, LoggingRepository>();
        builder.Services.AddScoped<ILogService, LogService>();
        
        // Exception handling services
        builder.Services.AddSingleton<IExceptionMessageService, ExceptionMessageService>();
        
        builder.Services.AddScoped<ITureRepository, TureRepository>();
        builder.Services.AddScoped<ITuraService, TuraService>();
        
        // User and Employee services
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        builder.Services.AddScoped<IEmployeeService, EmployeeService>();

        builder.Services.AddScoped<IDriverAssignmentRepository, DriverAssignmentRepository>();
        builder.Services.AddScoped<IDriverAssignmentService, DriverAssignmentService>();

        builder.Services.AddScoped<INalogVozacAccessService, NalogVozacAccessService>();

        builder.Services.AddScoped<INalogRepository, NalogRepository>();
        builder.Services.AddScoped<INalogService, NalogService>();

        builder.Services.AddScoped<INalogTroskoviRepository, NalogTroskoviRepository>();
        builder.Services.AddScoped<INalogTroskoviService, NalogTroskoviService>();

        builder.Services.AddScoped<INalogPrihodiRepository, NalogPrihodiRepository>();
        builder.Services.AddScoped<INalogPrihodiService, NalogPrihodiService>();

        builder.Services.AddScoped<INalogDokumentiRepository, NalogDokumentiRepository>();
        builder.Services.AddScoped<INalogDokumentiService, NalogDokumentiService>();

        builder.Services.AddScoped<IGorivoRepository, GorivoRepository>();
        builder.Services.AddScoped<IGorivoService, GorivoService>();

        if (!string.IsNullOrWhiteSpace(builder.Configuration["AppSettings:Blob"]))
            builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        else
            builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
        
        builder.Services.AddSingleton<IPdfTemplatePolicy, PdfTemplatePolicy>();

        // EKSPERIMENTALNO: QuestPDF servis za direktno PDF generisanje
        builder.Services.AddScoped<IQuestPdfNalogGenerator, QuestPdfNalogGenerator>();

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

        // Queue publisher service
        builder.Services.AddScoped<IQueuePublisher, QueuePublisher>();

        return builder;
    }

}
