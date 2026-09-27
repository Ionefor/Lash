using Lash.Web.ExceptionHandling;
using Lash.Users.Presentation;
using Serilog;

namespace Lash.Web.Extensions;

public static class WebServiceCollectionExtensions
{
    public static IServiceCollection AddLashWeb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSerilog((serviceProvider, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(serviceProvider)
            .Enrich.FromLogContext()
            .WriteTo.Console());

        services
            .AddLashHttp()
            .AddUsersPresentation();
        services.AddLashModules(configuration);
        services.AddHealthChecks();
        services.AddExceptionHandler(options => options.ExceptionHandler = _ => Task.CompletedTask);
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }
}
