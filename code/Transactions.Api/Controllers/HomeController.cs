using Transactions.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Transactions.Domain.Dtos;

namespace Transactions.Api.Controllers
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
                Email = User.Claims.Where(x => x.Type == ClaimTypes.Email).Select(x => x.Value).FirstOrDefault(),
                Claims = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value),
            };

            return Ok(user);
        }
    }
}
