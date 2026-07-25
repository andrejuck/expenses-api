using Transactions.Api.PresentationContracts.Expenses;
using Transactions.Domain.Models;
using Libs.Api.Models;
using Microsoft.AspNetCore.JsonPatch;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Api.DataContracts.Applications;

public interface ITransactionApplication
{
    Task CreateNewTransactionAsync(Guid userId, TransactionForm form);
    Task<PagedResponse<TransactionResponse>> GetPagedTransactionAsync(TransactionSearchParam searchParams, PagedRequest request, Guid userId);
    Task<List<string>> GetUserCategoriesAsync(Guid userId);
    Task UpdateTransactionAsync(Guid id, Guid userId, JsonPatchDocument<TransactionForm> patchForm);
    Task<TransactionResponse?> GetTransactionAsync(Guid id, Guid userId);
    Task DeleteTransactionAsync(Guid id, Guid userId);
    Task<PagedResponse<GroupedExpensesResponse>> GetGroupedPagedExpenseAsync(TransactionSearchParam searchParams, PagedRequest pagedRequest, Guid userId);
    Task<List<TransactionFileResponse>> GetAllExpensesAsync(TransactionSearchParam searchParams, Guid userId);
}