using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Expenses.Domain.Models;
using Libs.Api.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Expenses.Api.DataContracts.Applications;

public interface IExpenseApplication
{
    Task CreateNewExpenseAsync(Guid userId, ExpenseForm form);
    Task<PagedResponse<ExpenseResponse>> GetPagedExpenseAsync(ExpenseSearchParam searchParams, PagedRequest request, Guid userId);
    Task<List<string>> GetUserCategoriesAsync(Guid userId);
    Task UpdateExpenseAsync(Guid id, Guid userId, JsonPatchDocument<ExpenseForm> patchForm);
    Task<ExpenseResponse> GetExpenseAsync(Guid id, Guid userId);
    Task DeleteExpenseAsync(Guid id, Guid userId);
}