using FluentValidation;
using Lash.Users.Application.Features.Commands.RegisterClient;
using Lash.Users.Application.Features.Commands.RegisterMaster;
using Microsoft.Extensions.DependencyInjection;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<RegisterClientCommand, Guid>, RegisterClientHandler>();
        services.AddScoped<ICommandHandler<RegisterMasterCommand, Guid>, RegisterMasterHandler>();
        services.AddScoped<IValidator<RegisterClientCommand>, RegisterClientCommandValidator>();
        services.AddScoped<IValidator<RegisterMasterCommand>, RegisterMasterCommandValidator>();

        return services;
    }
}
