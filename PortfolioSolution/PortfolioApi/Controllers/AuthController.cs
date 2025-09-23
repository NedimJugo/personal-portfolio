using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Models.DTOs;
using PortfolioApi.Services.Interfaces;

namespace PortfolioApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var result = await _authService.LoginAsync(request);

                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for email: {Email}", request.Email);
                return StatusCode(500, new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var result = await _authService.RefreshTokenAsync(request);

                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var result = await _authService.LogoutAsync(request.RefreshToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }
    }
}
