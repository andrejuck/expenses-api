using Expenses.Api.Adapters;
using Expenses.Api.DataContracts.Applications;
using Expenses.Api.PresentationContracts.Expenses;
using Transactions.Domain.Models;
using Libs.Api.Extensions;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace Expenses.Api.Controllers;

[ApiController]
[Route("api/report")]
[Authorize]
public class ReportController : ControllerBase
{

    private readonly IExpenseApplication _expenseApplication;
    private readonly CustomClaimSettings _claimSettings;
    private readonly CsvHelperAdapter _csvAdapter;
    private Guid UserId => UserClaimsHelper.GetUserGuidIdFromClaims(User, _claimSettings);

    public ReportController(IExpenseApplication expenseApplication,
        IOptions<CustomClaimSettings> claimSettings,
        CsvHelperAdapter csvAdapter)
    {
        _expenseApplication = expenseApplication;
        _claimSettings = claimSettings.Value;
        _csvAdapter = csvAdapter;
    }

    [HttpGet("expenses/csv")]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> ExportExpenses([FromQuery] ExpenseSearchParam searchParams)
    {
        var result = await _expenseApplication.GetAllExpensesAsync(searchParams, UserId);

        if (result.IsNullOrEmpty()) return NoContent();

        var memoryStream = new MemoryStream();
        _csvAdapter.GenerateFile<ExpenseFileResponse, ExpenseMap>(memoryStream, result, HttpContext.GetCultureFromHeader());

        return File(memoryStream, "text/csv", $"{DateTime.Now.ToString("yyyyMMdd HHmmss")}");
    }
}
