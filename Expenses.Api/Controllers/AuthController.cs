using Libs.Auth.Dtos;
using Libs.Auth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
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
        public IActionResult Register(RegisterDto dto)
        {
            if (Users.Any(u => u.Username == dto.Username))
            {
                return Conflict(new { Message = "Username already exists." });
            }

            Users.Add(new User { Username = dto.Username, Password = dto.Password, Role = dto.Role });
            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var user = Users.FirstOrDefault(u => u.Username == dto.Username && u.Password == dto.Password);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid username or password." });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { Token = tokenHandler.WriteToken(token) });
        }
    }
}
