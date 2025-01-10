using Expenses.Api.Dtos;
using Libs.Auth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;


namespace Expenses.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private static List<User> Users = new();
        private readonly IConfiguration _configuration;
        private readonly byte[] _key;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            _key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        }

        [HttpPost("register")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            if (Users.Any(u => u.Email == dto.Email))
            {
                return Conflict(new { Message = "Email already exists." });
            }

            Users.Add(new User(dto.Email, dto.Username, dto.Password));
            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult Login(LoginDto dto)
        {
            var user = Users.FirstOrDefault(u => u.Email == dto.Email && u.Password == dto.Password);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            user.UpdateLoggedAt();

            var tokenHandler = new JwtSecurityTokenHandler();
            var roles = user.Roles.Select(x => new Claim(ClaimTypes.Role, x.ToString()));
            var claimIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Username)});
            claimIdentity.AddClaims(roles);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimIdentity,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { Token = tokenHandler.WriteToken(token) });
        }
    }
}
