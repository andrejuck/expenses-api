using AutoMapper;
using Expenses.Api.DataContracts;
using Expenses.Api.Dtos;
using Expenses.Api.PresentationContracts;
using Expenses.Api.Settings;
using Transactions.Domain.DataContracts;
using Libs.Auth.Helpers;
using Libs.Auth.Models;
using Libs.Auth.Models.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
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
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly EmailingSettings _emailSettings;
        private readonly CustomClaimSettings _claimSettings;
        private readonly IMapper _mapper;
        private readonly byte[] _key;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IConfiguration configuration,
            IUserRepository userRepository,
            IEmailService emailService,
            IOptions<EmailingSettings> settings,
            IOptions<CustomClaimSettings> claimSettings,
            IMapper mapper,
            ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            _userRepository = userRepository;
            _emailService = emailService;
            _emailSettings = settings.Value;
            _claimSettings = claimSettings.Value;
            _mapper = mapper;
            _logger = logger;
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
                var invalidMessage = string.Format(AuthMessages.INVALID_EMAIL_ALREADY_EXISTS, dto.Email);
                _logger.LogWarning(invalidMessage);
                return Conflict(new { Message = invalidMessage });
            }

            var user = new User(dto.Email, dto.Username);
            user.SetPassword(dto.Password);
            await _userRepository.AddAsync(user);

            if (_emailSettings.IsEnabled)
            {
                var token = CreateJwtToken(user);
                var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/auth/confirm-email?token={token}";
                var emailBody = $"<p>Olá {dto.Username},</p><p>Por favor, confirme seu e-mail clicando no link abaixo:</p>" +
                                $"<a href='{confirmationLink}'>Confirmar e-mail</a>";

                await _emailService.SendEmailAsync(dto.Email, "Confirmação de E-mail", emailBody);
            }

            var message = string.Format(AuthMessages.SUCCESS_REGISTERED, dto.Email);
            _logger.LogInformation(message);
            return Ok(new { Message = message });
        }

        [HttpPost("login")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<UserResponse>> Login(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                _logger.LogWarning(AuthMessages.INVALID_EMAIL_PASSWORD_LOGIN, dto.ToJson());
                return Unauthorized(new { Message = AuthMessages.INVALID_EMAIL_PASSWORD });
            }

            if (!user.VerifyPassword(dto.Password))
            {
                _logger.LogWarning(AuthMessages.INVALID_EMAIL_PASSWORD_LOGIN, dto.ToJson());
                return Unauthorized(new { Message = AuthMessages.INVALID_EMAIL_PASSWORD });
            }

            user.UpdateLoggedAt();
            //TODO - it should do a fire and forget notification to update db, because this is not relevant to login flow
            await _userRepository.UpdateAsync(user);

            var stringToken = CreateJwtToken(user);
            var response = _mapper.Map<UserResponse>(user);
            response.Token = stringToken;

            _logger.LogInformation(AuthMessages.SUCCESS_LOGIN, dto.Email);
            return Ok(response);
        }

        [HttpGet("confirm-email")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
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

            var claims = tokenHandler.ValidateToken(token, validationParameters, out var secToken);

            if (claims is null)
            {
                //TODO - Re-send confirmation email
                _logger.LogWarning(AuthMessages.INVALID_CONFIRMATION_EMAIL_EXPIRED + $" | {UserClaimsHelper.GetUserGuidIdFromClaims(claims, _claimSettings)}");
                return BadRequest(new { Message = AuthMessages.INVALID_CONFIRMATION_EMAIL_EXPIRED });
            }

            var userEmail = claims.FindFirst(ClaimTypes.Email).Value;
            var user = await _userRepository.GetByEmailAsync(userEmail);

            user.UpdateRegistrationStatus(RegistrationStatus.Approved);
            user.ConfirmEmail();
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation(AuthMessages.SUCCESS_CONFIRMED_EMAIL);
            return Ok(new { Message = AuthMessages.SUCCESS_CONFIRMED_EMAIL });
        }

        private string CreateJwtToken(User user)
        {
            //Creating jwt token with information related to the user logged such as roles, username and id
            var tokenHandler = new JwtSecurityTokenHandler();
            var roles = user.Roles.Select(x => new Claim(ClaimTypes.Role, x.ToString()));
            var claimIdentity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(_claimSettings.Identity, user.Id.ToString())
            });
            claimIdentity.AddClaims(roles);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "expenses-api.com",
                Audience = "expenses-api.com",
                Subject = claimIdentity,
                Expires = DateTime.UtcNow.AddDays(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = new { Token = tokenHandler.WriteToken(token) };
            return stringToken.Token;
        }

    }
}
