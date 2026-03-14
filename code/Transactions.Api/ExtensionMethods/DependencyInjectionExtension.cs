using Transactions.Api.Adapters;
using Transactions.Api.Application;
using Transactions.Api.DataContracts;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.Services;
using Transactions.Domain.DataContracts;
using Transactions.Infra.Repositories;
using Libs.Api.Adapters;
using Libs.Api.ErrorHandling;

namespace Transactions.Api.ExtensionMethods;

public static class DependencyInjectionExtension
{

    public static IServiceCollection AddExpensesDependencies(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IModuleApplication, ModuleApplication>();

        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IPaymentMethodApplication, PaymentMethodApplication>();

        services.AddScoped<ITransactionApplication, TransactionApplication>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPaginationAdapter, PaginationAdapter>();
        services.AddScoped<IErrorService, ErrorService>();

        services.AddSingleton<CsvHelperAdapter>();
        //services.AddSingleton<IStringLocalizer>(sp =>
        //{
        //    var factory = sp.GetRequiredService<IStringLocalizerFactory>();
        //    return factory.Create(typeof(SharedResources));
        //});
        return services;
    }
}