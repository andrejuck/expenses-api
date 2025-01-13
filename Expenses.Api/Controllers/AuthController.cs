using Expenses.Api.Dtos;
using Expenses.Domain.DataContract;
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
    //TODO - Remove repository from controller and implement an orchestration layer
    public class AuthController : ControllerBase
    {
        private static List<User> Users = new();
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly byte[] _key;

        public AuthController(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration;
            _key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (Users.Any(u => u.Email == dto.Email))
            {
                return Conflict(new { Message = "Email already exists." });
            }

            var user = new User(dto.Email, dto.Username, dto.Password); 
            await _userRepository.AddAsync(user);

            Users.Add(user);
            return Ok(new { Message = $"User {user.Email} registered successfully." });
        }

        [HttpPost("login")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAndPassword(dto.Email, dto.Password);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            user.UpdateLoggedAt();
            //TODO - it should do a fire and forget notification to update db, because this is not relevant to login flow
            await _userRepository.UpdateAsync(user);

            //Creating jwt token with information related to the user logged such as roles and username
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
