using Transactions.Api.DataContracts.Applications;
using Transactions.Api.PresentationContracts.Expenses;
using Transactions.Domain.Models;
using HotChocolate.Authorization;
using Libs.Api.Models;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.Extensions.Options;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Api.PresentationContracts.GraphQL;

[Authorize]
public class ExpenseReportQuery
{
    private readonly ITransactionApplication _transactionApplication;
    private readonly Guid UserId;

    public ExpenseReportQuery(
        ITransactionApplication transactionApplication,
        IOptions<CustomClaimSettings> claimSettings,
        [Service] IHttpContextAccessor context)
    {
        _transactionApplication = transactionApplication;
        UserId = UserClaimsHelper.GetUserGuidIdFromClaims(context.HttpContext.User, claimSettings.Value);
    }

    public async Task<PagedResponse<TransactionResponse>> GetExpenses(TransactionSearchParam searchParams, PagedRequest request)
    {
        var response = await _transactionApplication.GetPagedTransactionAsync(searchParams, request, UserId);
        return response;
    }

    public async Task<PagedResponse<GroupedExpensesResponse>> GetGroupedExpenses(TransactionSearchParam searchParam, PagedRequest request)
    {
        var response = await _transactionApplication.GetGroupedPagedExpenseAsync(searchParam, request, UserId);
        return response;
    }
}