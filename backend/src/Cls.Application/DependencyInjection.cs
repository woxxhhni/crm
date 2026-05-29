using Cls.Application.Auth.Commands;
using Cls.Application.Auth.Validators;
using Cls.Application.Abstractions;
using Cls.Application.Clients.Guards;
using Cls.Application.OrderLogs.Abstractions;
using Cls.Application.OrderLogs.Services;
using Cls.Application.Orders.Services;
using Cls.Application.Providers.Guards;
using Cls.Application.Users.Services;
using Cls.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cls.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IHostApplicationBuilder applicationBuilder)
    {
        var services = applicationBuilder.Services;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<AuthenticateCommand>();
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Common.Behaviors.PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behaviors.ValidationBehavior<,>));
        services.AddValidatorsFromAssemblyContaining<AuthenticateCommandValidator>();

        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IOrderContractEnricher, OrderContractEnricher>();
        services.AddScoped<IOrderLogService, OrderLogService>();
        services.AddScoped<IDeletionGuard<Client>, ClientDeletionGuard>();
        services.AddScoped<IDeletionGuard<Provider>, ProviderDeletionGuard>();

        return services;
    }
}
