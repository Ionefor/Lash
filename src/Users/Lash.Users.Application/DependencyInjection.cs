using FluentValidation;
using Lash.Users.Application.Features.Commands.RegisterClient;
using Lash.Users.Application.Features.Commands.RegisterMaster;
using Lash.Users.Application.Features.Commands.Login;
using Lash.Users.Application.Features.Commands.Logout;
using Lash.Users.Application.Features.Commands.Refresh;
using Lash.Users.Application.Features.Commands.ConfirmEmail;
using Lash.Users.Application.Features.Commands.ResendEmailConfirmation;
using Lash.Users.Application.Features.Commands.RequestPasswordReset;
using Lash.Users.Application.Features.Commands.ResetPassword;
using Lash.Users.Application.Features.Commands.ChangePassword;
using Lash.Users.Application.Features.Queries.GetCurrentUser;
using Microsoft.Extensions.DependencyInjection;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<RegisterClientCommand, Guid>, RegisterClientHandler>();
        services.AddScoped<ICommandHandler<RegisterMasterCommand, Guid>, RegisterMasterHandler>();
        services.AddScoped<ICommandHandler<LoginCommand, Models.AuthTokens>, LoginHandler>();
        services.AddScoped<ICommandHandler<RefreshTokensCommand, Models.AuthTokens>, RefreshTokensHandler>();
        services.AddScoped<ICommandHandler<LogoutCommand>, LogoutHandler>();
        services.AddScoped<ICommandHandler<ConfirmEmailCommand>, ConfirmEmailHandler>();
        services.AddScoped<ICommandHandler<ResendEmailConfirmationCommand>, ResendEmailConfirmationHandler>();
        services.AddScoped<ICommandHandler<RequestPasswordResetCommand>, RequestPasswordResetHandler>();
        services.AddScoped<ICommandHandler<ResetPasswordCommand>, ResetPasswordHandler>();
        services.AddScoped<ICommandHandler<ChangePasswordCommand>, ChangePasswordHandler>();
        services.AddScoped<IQueryHandler<GetCurrentUserQuery, Models.UserProfile>, GetCurrentUserHandler>();
        services.AddScoped<IValidator<RegisterClientCommand>, RegisterClientCommandValidator>();
        services.AddScoped<IValidator<RegisterMasterCommand>, RegisterMasterCommandValidator>();
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
        services.AddScoped<IValidator<ResetPasswordCommand>, ResetPasswordCommandValidator>();
        services.AddScoped<IValidator<ChangePasswordCommand>, ChangePasswordCommandValidator>();

        return services;
    }
}
