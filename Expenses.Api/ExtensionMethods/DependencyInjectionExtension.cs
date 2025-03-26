using Expenses.Api.Application;
using Expenses.Api.DataContracts;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.Services;
using Expenses.Domain.DataContracts;
using Expenses.Infra.Repositories;
using Libs.Api.Adapters;
using Libs.Api.Adapters.Resources;
using Libs.Api.ErrorHandling;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;

namespace Expenses.Api.ExtensionMethods;

public static class DependencyInjectionExtension
{

    public static IServiceCollection AddExpensesDependencies(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IModuleApplication, ModuleApplication>();

        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IPaymentMethodApplication, PaymentMethodApplication>();

        services.AddScoped<IExpenseApplication, ExpenseApplication>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPaginationAdapter, PaginationAdapter>();
        services.AddScoped<IErrorService, ErrorService>();

        services.AddSingleton<CsvHelperAdapter>();
        services.AddSingleton<IStringLocalizer>(sp =>
        {
            var factory = sp.GetRequiredService<IStringLocalizerFactory>();
            return factory.Create(typeof(SharedResources));
        });
        return services;
    }
}