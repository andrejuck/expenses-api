using Transactions.Api.Adapters;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.PresentationContracts.Expenses;
using Transactions.Domain.Models;
using Libs.Api.Extensions;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using Transactions.Domain.Models.Transaction;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/report")]
[Authorize]
public class ReportController : ControllerBase
{

    private readonly ITransactionApplication _transactionApplication;
    private readonly CustomClaimSettings _claimSettings;
    private readonly CsvHelperAdapter _csvAdapter;
    private Guid UserId => UserClaimsHelper.GetUserGuidIdFromClaims(User, _claimSettings);

    public ReportController(ITransactionApplication transactionApplication,
        IOptions<CustomClaimSettings> claimSettings,
        CsvHelperAdapter csvAdapter)
    {
        _transactionApplication = transactionApplication;
        _claimSettings = claimSettings.Value;
        _csvAdapter = csvAdapter;
    }

    [HttpGet("expenses/csv")]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> ExportExpenses([FromQuery] TransactionSearchParam searchParams)
    {
        var result = await _transactionApplication.GetAllExpensesAsync(searchParams, UserId);

        if (result.IsNullOrEmpty()) return NoContent();

        var memoryStream = new MemoryStream();
        _csvAdapter.GenerateFile<TransactionFileResponse, TransactionMap>(memoryStream, result, HttpContext.GetCultureFromHeader());

        return File(memoryStream, "text/csv", $"{DateTime.Now.ToString("yyyyMMdd HHmmss")}");
    }
}
