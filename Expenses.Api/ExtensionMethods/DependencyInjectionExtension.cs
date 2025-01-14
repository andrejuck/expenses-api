using Expenses.Api.DataContracts;
using Expenses.Api.Services;
using Expenses.Domain.DataContract;
using Expenses.Infra.Repositories;

namespace Expenses.Api.ExtensionMethods;

public static class DependencyInjectionExtension {

    public static IServiceCollection AddExpensesDependencies(this IServiceCollection services) {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}