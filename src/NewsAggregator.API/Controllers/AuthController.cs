using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NewsAggregator.Application.DTOs;
using NewsAggregator.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NewsAggregator.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserAppService _userAppService;
        private readonly IConfiguration _config;
        private readonly ITokenAppService _tokenAppService;

        public AuthController(IUserAppService userAppService, IConfiguration config, ITokenAppService tokenAppService)
        {
            _userAppService = userAppService;
            _config = config;
            _tokenAppService = tokenAppService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string username)
        {
            try
            {
                var result = await _userAppService.GetByUsername(username);

                if (!result.Success)
                {
                    if (result.ErrorMessage?.Contains("User name can not be null or empty.") == true)
                    {
                        return BadRequest(result.ErrorMessage);
                    }

                    if (result.ErrorMessage?.Contains("User name contains invalid characters.") == true)
                    {
                        return BadRequest(result.ErrorMessage);
                    }

                    if (result.ErrorMessage?.Contains("Not found!") == true)
                    {
                        return NotFound(result.ErrorMessage);
                    }

                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto user)
        {
            try
            {
                var result = await _userAppService.CreateUser(user);

                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return CreatedAtAction(nameof(Get), new { username = result.Data!.Username }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto user)
        {
            try
            {
                var result = await _userAppService.GetToken(user);
                if (!result.Success)
                    return BadRequest(result.ErrorMessage);

                return Ok(new { result.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto model)
        {
            try
            {
                string jwtkey = Environment.GetEnvironmentVariable("JWT_Key") ?? _config["Jwt:Key"]!;
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(model.AccessToken, new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = false, // Allow expired token
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtkey!))
                }, out _);

                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

                var request = new RefreshTokenRequestProcessDto()
                {
                    AccessToken = model.AccessToken,
                    UserId = userId,
                    RefreshToken = model.RefreshToken,
                };
                var result = await _tokenAppService.RefreshToken(request);

                if (!result.Success)
                    return BadRequest(result.ErrorMessage);

                return Ok(new { result.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
