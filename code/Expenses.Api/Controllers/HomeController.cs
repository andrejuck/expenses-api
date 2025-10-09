using Expenses.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Expenses.Api.Controllers
{
    [ApiController]
    [Route("api/home")]
    [Authorize(Policy = "AdminOnly")]
    public class HomeController : Controller
    {

        [HttpGet]
        public ActionResult<UserDto> GetLoggedUser()
        {
            var user = new UserDto()
            {
                Username = User.Identity.Name,
                Claims = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value),
            };

            return Ok(user);
        }
    }
}
