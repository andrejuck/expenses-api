using Expenses.Api.DataContracts;
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
        // private static List<User> Users = new();
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly byte[] _key;

        public AuthController(IConfiguration configuration, IUserRepository userRepository, IEmailService emailService)
        {
            _configuration = configuration;
            _key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            _userRepository = userRepository;
            _emailService = emailService;
        }

        [HttpPost("register")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _userRepository.GetByEmailAsync(dto.Email) != null)
            {
                return Conflict(new { Message = "Email already exists." });
            }

            var user = new User(dto.Email, dto.Username); 
            user.SetPassword(dto.Password);
            await _userRepository.AddAsync(user);

            var token = CreateJwtToken(user);
            var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/auth/confirm-email?token={token}";
            var emailBody = $"<p>Olá {dto.Username},</p><p>Por favor, confirme seu e-mail clicando no link abaixo:</p>" +
                            $"<a href='{confirmationLink}'>Confirmar e-mail</a>";

            await _emailService.SendEmailAsync(dto.Email, "Confirmação de E-mail", emailBody);
            return Ok(new { Message = $"User {user.Email} registered successfully." });
        }

        [HttpPost("login")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            if (!user.VerifyPassword(dto.Password))
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            user.UpdateLoggedAt();
            //TODO - it should do a fire and forget notification to update db, because this is not relevant to login flow
            await _userRepository.UpdateAsync(user);

            object stringToken = CreateJwtToken(user);

            return Ok(stringToken);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            var key = new SymmetricSecurityKey(_key);

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "expenses-api.com",
                ValidAudience = "expenses-api.com",
                IssuerSigningKey = key
            };

            var isValid = tokenHandler.ValidateToken(token, validationParameters, out _);

            if(isValid == null) {
                return BadRequest(new { Message = "Link expirado!" });
            }

            var userEmail = isValid.FindFirst(ClaimTypes.Email).Value;
            var user = await _userRepository.GetByEmailAsync(userEmail);

            user.UpdateRegistrationStatus(RegistrationStatus.Approved);
            await _userRepository.UpdateAsync(user);

            return Ok(new { Message = "E-mail confirmado com sucesso!" });
        }

        private string CreateJwtToken(User user)
        {
            //Creating jwt token with information related to the user logged such as roles and username
            var tokenHandler = new JwtSecurityTokenHandler();
            var roles = user.Roles.Select(x => new Claim(ClaimTypes.Role, x.ToString()));
            var claimIdentity = new ClaimsIdentity(new[] { 
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            });
            claimIdentity.AddClaims(roles);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "expenses-api.com",
                Audience = "expenses-api.com",
                Subject = claimIdentity,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = new { Token = tokenHandler.WriteToken(token) };
            return stringToken.Token;
        }

    }
}
