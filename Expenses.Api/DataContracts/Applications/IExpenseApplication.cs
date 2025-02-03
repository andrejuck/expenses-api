using Expenses.Api.PresentationContracts;
using Expenses.Api.PresentationContracts.Forms;
using Libs.Api.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Expenses.Api.DataContracts.Applications;

public interface IExpenseApplication
{
    Task CreateNewExpenseAsync(Guid userId, ExpenseForm form);
    Task<PagedResponse<ExpenseResponse>> GetPagedExpense(PagedRequest pagedRequest, Guid userId);
    Task UpdateExpenseAsync(Guid id, Guid userId, JsonPatchDocument<ExpenseForm> patchForm);
}