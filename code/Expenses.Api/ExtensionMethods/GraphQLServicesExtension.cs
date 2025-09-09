using Expenses.Api.PresentationContracts.GraphQL;

namespace Expenses.Api.ExtensionMethods;
public static class GraphQLServicesExtension
{

    public static IServiceCollection AddGraphQL(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddQueryType<ExpenseReportQuery>()
            .AddAuthorization();

        services.AddHttpContextAccessor();
        services.AddScoped<ExpenseReportQuery>();

        return services;
    }
}