using System.Net;
using System.Security.Claims;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Transactions.Api.DataContracts.Applications;
using Transactions.Api.PresentationContracts.Families;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/family")]
[Authorize]
public class FamilyController : ControllerBase
{
    private readonly IFamilyApplication _application;
    private readonly CustomClaimSettings _claimSettings;
    private Guid UserId => UserClaimsHelper.GetUserGuidIdFromClaims(User, _claimSettings);
    private string? UserName => User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Name))?.Value;
    public FamilyController(IFamilyApplication application,
        IOptions<CustomClaimSettings> claimSettings)
    {
        _application = application;
        _claimSettings = claimSettings.Value;
    }

    [HttpGet("{id:guid}", Name = "FetchFamilyById")]
    [ProducesResponseType(typeof(FamilyResponse), (int)HttpStatusCode.OK)]
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
    public async Task<ActionResult<List<FamilyResponse>>> FetchUserFamiliesAsync()
    {
        var result = await _application.FetchUserFamiliesAsync(UserId);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(FamilyResponse), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> CreateFamilyAsync([FromBody] FamilyForm form)
    {
        var response = await _application.CreateFamilyAsync(form, UserId, UserName ?? string.Empty);
        return CreatedAtRoute("FetchFamilyById", new { id = response }, response);
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

        return AcceptedAtRoute("FetchFamilyById", new { id = response }, response);
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