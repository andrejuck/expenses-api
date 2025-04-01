using Expenses.Api.PresentationContracts.Expenses;
using Expenses.Domain.Models;
using Libs.Api.Models;
using Microsoft.AspNetCore.JsonPatch;
using MongoDB.Bson;

namespace Expenses.Api.DataContracts.Applications;

public interface IExpenseApplication
{
    Task CreateNewExpenseAsync(Guid userId, ExpenseForm form);
    Task<PagedResponse<ExpenseResponse>> GetPagedExpenseAsync(ExpenseSearchParam searchParams, PagedRequest request, Guid userId);
    Task<List<string>> GetUserCategoriesAsync(Guid userId);
    Task UpdateExpenseAsync(Guid id, Guid userId, JsonPatchDocument<ExpenseForm> patchForm);
    Task<ExpenseResponse> GetExpenseAsync(Guid id, Guid userId);
    Task DeleteExpenseAsync(Guid id, Guid userId);
    Task<PagedResponse<GroupedExpensesResponse>> GetGroupedPagedExpenseAsync(ExpenseSearchParam searchParams, PagedRequest pagedRequest, Guid userId);
    Task<List<ExpenseFileResponse>> GetAllExpensesAsync(ExpenseSearchParam searchParams, Guid userId);
}