using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Services.Database.Entities;
using Portfolio.Services.Database;
using Portfolio.Services.Interfaces;
using System.Security.Claims;
using Portfolio.Models.Responses.Auth;
using Portfolio.Models.Requests.Auth;

namespace Portfolio.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            ApplicationDbContext context,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] Portfolio.Models.Requests.Auth.LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized("Invalid credentials");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                        return Unauthorized("Account is locked out");

                    return Unauthorized("Invalid credentials");
                }

                // Generate tokens
                var roles = await _userManager.GetRolesAsync(user);
                var accessToken = _jwtService.GenerateJwtToken(user, roles);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Save refresh token to database
                refreshToken.UserId = user.Id;
                _context.RefreshTokens.Add(refreshToken);

                // Update last login
                user.LastLoginAt = DateTimeOffset.UtcNow;
                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();

                var response = new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60), // Should match JWT expiration
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        FullName = user.FullName,
                        UserName = user.UserName!,
                        Roles = roles.ToList()
                    }
                };

                _logger.LogInformation("User {Email} logged in successfully", request.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", request.Email);
                return BadRequest("An error occurred during login");
            }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegisterResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] Portfolio.Models.Requests.Auth.RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest("User with this email already exists");
                }

                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                // Assign default role (you can customize this)
                await _userManager.AddToRoleAsync(user, "User");

                var response = new RegisterResponse
                {
                    Message = "User registered successfully",
                    UserId = user.Id
                };

                _logger.LogInformation("New user registered: {Email}", request.Email);
                return CreatedAtAction(nameof(Login), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", request.Email);
                return BadRequest("An error occurred during registration");
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RefreshTokenResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
                if (principal == null)
                {
                    return Unauthorized("Invalid access token");
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized("Invalid token claims");
                }

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || !user.IsActive)
                {
                    return Unauthorized("User not found or inactive");
                }

                // Validate refresh token
                var isValidRefreshToken = await _jwtService.ValidateRefreshTokenAsync(request.RefreshToken, userId);
                if (!isValidRefreshToken)
                {
                    return Unauthorized("Invalid or expired refresh token");
                }

                // Generate new tokens
                var roles = await _userManager.GetRolesAsync(user);
                var newAccessToken = _jwtService.GenerateJwtToken(user, roles);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // Revoke old refresh token and save new one
                await _jwtService.RevokeRefreshTokenAsync(request.RefreshToken);

                newRefreshToken.UserId = userId;
                _context.RefreshTokens.Add(newRefreshToken);
                await _context.SaveChangesAsync();

                var response = new RefreshTokenResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken.Token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return BadRequest("An error occurred during token refresh");
            }
        }

        /// <summary>
        /// Logout user and revoke refresh token
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    if (!string.IsNullOrEmpty(request.RefreshToken))
                    {
                        await _jwtService.RevokeRefreshTokenAsync(request.RefreshToken);
                    }
                    else
                    {
                        // If no specific refresh token provided, revoke all user's refresh tokens
                        await _jwtService.RevokeAllUserRefreshTokensAsync(userId);
                    }
                }

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return BadRequest("An error occurred during logout");
            }
        }

        /// <summary>
        /// Get current user info
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserInfo), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || !user.IsActive)
                {
                    return Unauthorized();
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userInfo = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    UserName = user.UserName!,
                    Roles = roles.ToList()
                };

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return BadRequest("An error occurred while getting user information");
            }
        }

        /// <summary>
        /// Revoke all refresh tokens for current user
        /// </summary>
        [HttpPost("revoke-all-tokens")]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RevokeAllTokens()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    await _jwtService.RevokeAllUserRefreshTokensAsync(userId);
                    return Ok(new { message = "All refresh tokens revoked successfully" });
                }

                return BadRequest("Unable to identify user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all tokens");
                return BadRequest("An error occurred while revoking tokens");
            }
        }
    }
}
