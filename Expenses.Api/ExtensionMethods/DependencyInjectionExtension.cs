using Expenses.Api.Application;
using Expenses.Api.DataContracts;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.Services;
using Expenses.Domain.DataContracts;
using Expenses.Infra.Repositories;
using Libs.Api.Adapters;

namespace Expenses.Api.ExtensionMethods;

public static class DependencyInjectionExtension {

    public static IServiceCollection AddExpensesDependencies(this IServiceCollection services) {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPaginationAdapter, PaginationAdapter>();
        services.AddScoped<IPaymentMethodApplication, PaymentMethodApplication>();

        return services;
    }
}