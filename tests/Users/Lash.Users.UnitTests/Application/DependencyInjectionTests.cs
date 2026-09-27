using FluentValidation;
using Lash.Users.Application;
using Microsoft.Extensions.DependencyInjection;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.UnitTests.Application;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddUsersApplication_RegistersAllHandlersAndValidatorsAsScopedServices()
    {
        var services = new ServiceCollection();

        services.AddUsersApplication();

        var expectedContracts = typeof(DependencyInjection).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type.GetInterfaces())
            .Where(IsApplicationContract);

        Assert.All(expectedContracts, contract =>
        {
            var registration = Assert.Single(services, descriptor =>
                descriptor.ServiceType == contract);

            Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
        });
    }

    private static bool IsApplicationContract(Type type) =>
        type.IsGenericType &&
        (type.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
         type.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
         type.GetGenericTypeDefinition() == typeof(IQueryHandler<,>) ||
         type.GetGenericTypeDefinition() == typeof(IValidator<>));
}
