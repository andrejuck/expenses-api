using Expenses.Api.PresentationContracts.Forms;
using Microsoft.AspNetCore.JsonPatch;

namespace Expenses.Api.DataContracts.Applications;

public interface IExpenseApplication
{
    Task CreateNewExpenseAsync(Guid userId, ExpenseForm form);
    Task UpdateExpenseAsync(Guid id, Guid userId, JsonPatchDocument<ExpenseForm> patchForm);
}