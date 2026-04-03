using Transactions.Api.Adapters;
using Transactions.Api.Application;
using Transactions.Api.DataContracts;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.Services;
using Transactions.Domain.DataContracts;
using Transactions.Infra.Repositories;
using Libs.Api.Adapters;
using Libs.Api.ErrorHandling;
using Transactions.Api.DataContracts.Adapters;

namespace Transactions.Api.ExtensionMethods;

public static class DependencyInjectionExtension
{

    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddTransient<IModuleRepository, ModuleRepository>();
        services.AddTransient<IModuleApplication, ModuleApplication>();

        services.AddTransient<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddTransient<IPaymentMethodApplication, PaymentMethodApplication>();

        services.AddTransient<ITransactionApplication, TransactionApplication>();
        services.AddTransient<IExpenseRepository, ExpenseRepository>();
        
        services.AddTransient<IAccountAdapter, AccountAdapter>();
        services.AddTransient<IAccountApplication, AccountApplication>();
        services.AddTransient<IAccountRepository, AccountRepository>();
        
        services.AddTransient<IAccountAdapter, AccountAdapter>();
        services.AddTransient<IAccountApplication, AccountApplication>();
        services.AddTransient<IAccountRepository, AccountRepository>();
        
        services.AddTransient<IFamilyAdapter, FamilyAdapter>();
        services.AddTransient<IFamilyApplication, FamilyApplication>();
        services.AddTransient<IFamilyRepository, FamilyRepository>();

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