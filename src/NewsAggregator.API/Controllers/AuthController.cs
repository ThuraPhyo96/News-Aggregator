using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Application.DTOs;
using NewsAggregator.Application.Interfaces;

namespace NewsAggregator.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserAppService _userAppService;

        public AuthController(IUserAppService userAppService)
        {
            _userAppService = userAppService;
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
                    return BadRequest(result.ErrorMessage);

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
    }
}
