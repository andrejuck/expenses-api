using Transactions.Api.Helpers;
using Transactions.Api.PresentationContracts;
using Transactions.Domain.DataContracts;
using Transactions.Domain.Models;
using Libs.Api.Adapters;
using Libs.Api.Models;
using Libs.Auth.Helpers;
using Libs.Auth.Models.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/user")]
[Authorize(Policy = "AdminOnly")]
public class UserController : ControllerBase
{

    private readonly IUserRepository _userRepository;
    private readonly IPaginationAdapter _pageAdapter;
    private readonly ILogger<UserController> _logger;
    private readonly CustomClaimSettings _claimSettings;
    private Guid UserId => UserClaimsHelper.GetUserGuidIdFromClaims(User, _claimSettings);

    public UserController(
        IUserRepository userRepository,
        IPaginationAdapter pageAdapter,
        ILogger<UserController> logger,
        IOptions<CustomClaimSettings> options)
    {
        _userRepository = userRepository;
        _pageAdapter = pageAdapter;
        _logger = logger;
        _claimSettings = options.Value;
    }

    [HttpPatch("approve/{id}")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> ApproveUserRegistration(Guid id)
    {

        var user = await _userRepository.FindByIdAsync(id);
        user.UpdateRegistrationStatus(RegistrationStatus.Approved);
        user.ConfirmEmail();

        await _userRepository.UpdateAsync(user);
        _logger.LogInformation("User {user} was approved by {userId}", user.Username, UserId);
        return Accepted();
    }

    [HttpPatch("deny/{id}")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> DenyUserRegistration(Guid id)
    {

        var user = await _userRepository.FindByIdAsync(id);
        user.UpdateRegistrationStatus(RegistrationStatus.Denied);

        //TODO - Send an email to the user asking to get in contact with support.
        //Admin should justify the motive of this denial

        await _userRepository.UpdateAsync(user);
        _logger.LogInformation("User {user} was denied by {userId}", user.Username, UserId);
        return Accepted();
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetPagedUserList(
        [FromQuery] UserSearchParam searchParams,
        [FromQuery] PagedRequest request)
    {
        var users = await _userRepository.GetAllPagedAsync<UserResponse>(searchParams, request);
        var totalUsers = await _userRepository.GetAllCountAsync(searchParams);
        var response = _pageAdapter.ConvertToResponse(request, totalUsers, users);

        _logger.LogInformation(Messages.LOG_GET_PAGED_MULTIPLE_MESSAGE, nameof(UserResponse), users.Count, totalUsers, UserId);
        return Ok(response);
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public ActionResult<List<string>> GetAllRegistrationStatus()
    {

        return Ok(Enum.GetNames<RegistrationStatus>().ToList());
    }
}