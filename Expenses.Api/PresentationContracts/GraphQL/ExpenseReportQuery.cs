using Expenses.Api.DataContracts.Applications;
using Expenses.Api.PresentationContracts.Expenses;
using Expenses.Domain.Models;
using HotChocolate.Authorization;
using Libs.Api.Models;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.Extensions.Options;

namespace Expenses.Api.PresentationContracts.GraphQL;

[Authorize]
public class ExpenseReportQuery
{
    private readonly IExpenseApplication _expenseApplication;
    private readonly Guid UserId;

    public ExpenseReportQuery(
        IExpenseApplication expenseApplication,
        IOptions<CustomClaimSettings> claimSettings,
        [Service] IHttpContextAccessor context)
    {
        _expenseApplication = expenseApplication;
        UserId = UserClaimsHelper.GetUserGuidIdFromClaims(context.HttpContext.User, claimSettings.Value);
    }

    public async Task<PagedResponse<ExpenseResponse>> GetExpenses(ExpenseSearchParam searchParams, PagedRequest request)
    {
        var response = await _expenseApplication.GetPagedExpenseAsync(searchParams, request, UserId);
        return response;
    }

    public async Task<PagedResponse<GroupedExpensesResponse>> GetGroupedExpenses(ExpenseSearchParam searchParam, PagedRequest request)
    {
        var response = await _expenseApplication.GetGroupedPagedExpenseAsync(searchParam, request, UserId);
        return response;
    }
}