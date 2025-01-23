using System.Net;
using AutoMapper;
using Expenses.Api.Adapters;
using Expenses.Api.DataContracts;
using Expenses.Api.PresentationContracts;
using Expenses.Domain.DataContracts;
using Libs.Api.Adapters;
using Libs.Api.Models;
using Libs.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expenses.Api.Controllers;

[ApiController]
[Route("api/user")]
[Authorize(Policy = "AdminOnly")]
public class UserController : ControllerBase
{

    private readonly IUserRepository _userRepository;
    private readonly IPaginationAdapter _pageAdapter;
    private readonly IMapper _mapper;

    public UserController(IUserRepository userRepository, IPaginationAdapter pageAdapter, IMapper mapper)
    {
        _userRepository = userRepository;
        _pageAdapter = pageAdapter;
        _mapper = mapper;
    }

    [HttpPatch("approve")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> ApproveUserRegistration([FromQuery] Guid userId)
    {

        var user = await _userRepository.FindByIdAsync(userId);
        user.UpdateRegistrationStatus(RegistrationStatus.Approved);
        user.ConfirmEmail();

        await _userRepository.UpdateAsync(user);

        return Accepted();
    }

    [HttpPatch("deny")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> DenyUserRegistration([FromQuery] Guid userId)
    {

        var user = await _userRepository.FindByIdAsync(userId);
        user.UpdateRegistrationStatus(RegistrationStatus.Denied);

        //TODO - Send an email to the user asking to get in contact with support.
        //Admin should justify the motive of this denial

        await _userRepository.UpdateAsync(user);

        return Accepted();
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetPaginatedUserList(
        [FromQuery] PagedRequest request)
    {
        var users = await _userRepository.GetAllPagedAsync(request);
        var totalUsers = await _userRepository.GetAllCountAsync(request);
        var usersResponse = _mapper.Map<List<UserResponse>>(users);
        var response = _pageAdapter.ConvertToResponse(request, totalUsers, usersResponse);

        return Ok(response);
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public ActionResult<List<string>> GetAllRegistrationStatus() {
        
        return Ok(Enum.GetNames<RegistrationStatus>().ToList());
    }
}