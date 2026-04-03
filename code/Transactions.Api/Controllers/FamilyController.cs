using System.Net;
using Libs.Api.Models;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.PresentationContracts;
using Transactions.Api.PresentationContracts.Accounts;
using Transactions.Api.PresentationContracts.Families;
using Transactions.Api.PresentationContracts.Forms;
using Transactions.Domain.Models.Accounts;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/family")]
[Authorize]
public class FamilyController : ControllerBase
{
    private readonly IFamilyApplication _application;
    private readonly CustomClaimSettings _claimSettings;
    private Guid UserId => UserClaimsHelper.GetUserGuidIdFromClaims(User, _claimSettings);
    public FamilyController(IFamilyApplication application,
        IOptions<CustomClaimSettings> claimSettings)
    {
        _application = application;
        _claimSettings = claimSettings.Value;
    }

    [HttpGet("{id:guid}", Name = "FetchFamilyById")]
    [ProducesResponseType(typeof(List<FamilyResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<FamilyResponse>> FetchFamilyByIdAsync(Guid id)
    {
        var result = await _application.FetchFamilyByIdAsync(id, UserId);
        return Ok(result);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(List<FamilyResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<List<FamilyResponse>>> FetchUserFamiliesAsync(
        [FromQuery] AccountSearchParam searchParams,
        [FromQuery] PagedRequest pagedRequest)
    {
        var result = await _application.FetchUserFamiliesAsync(searchParams, UserId);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(FamilyResponse), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> CreateFamilyAsync([FromBody] FamilyForm form)
    {
        var response = await _application.CreateFamilyAsync(form, UserId);
        return CreatedAtRoute("FetchFamilyById", new { id = response.Id }, response);
    }
    
    [HttpPut("{familyId:guid}")]
    [ProducesResponseType(typeof(FamilyResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> UpdateFamilyAsync(Guid familyId, [FromBody] FamilyForm form)
    {
        var response = await _application.UpdateFamilyAsync(familyId, form, UserId);
        
        if(response is null)
            return Ok(response);

        return AcceptedAtRoute("FetchFamilyById", new { id = response.Id }, response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> DeleteModule(Guid id)
    {
        await _application.DeleteByIdAsync(id, UserId);
        return NoContent();
    }
}