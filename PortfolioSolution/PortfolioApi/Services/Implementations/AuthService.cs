using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PortfolioApi.Models.DTOs;
using PortfolioApi.Models.Entities;
using PortfolioApi.Models;
using PortfolioApi.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PortfolioApi.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || !user.IsActive)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid credentials"
                    };
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid credentials"
                    };
                }

                // Update last login
                user.LastLoginAt = DateTimeOffset.UtcNow;
                await _userManager.UpdateAsync(user);

                // Generate tokens
                var jwtToken = await GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
                };

                await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshTokenEntity);
                await _unitOfWork.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

                return new ApiResponse<AuthResponseDto>
                {
                    Data = new AuthResponseDto
                    {
                        Token = jwtToken,
                        RefreshToken = refreshToken,
                        User = userDto,
                        ExpiresIn = _jwtSettings.ExpirationMinutes * 60
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", request.Email);
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            try
            {
                var refreshTokenEntity = await _unitOfWork.Repository<RefreshToken>()
                    .GetFirstAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked);

                if (refreshTokenEntity == null || refreshTokenEntity.ExpiresAt <= DateTimeOffset.UtcNow)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token"
                    };
                }

                var user = await _userManager.FindByIdAsync(refreshTokenEntity.UserId.ToString());
                if (user == null || !user.IsActive)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "User not found or inactive"
                    };
                }

                // Revoke old refresh token
                refreshTokenEntity.IsRevoked = true;

                // Generate new tokens
                var jwtToken = await GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                // Save new refresh token
                var newRefreshTokenEntity = new RefreshToken
                {
                    Token = newRefreshToken,
                    UserId = user.Id,
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
                };

                await _unitOfWork.Repository<RefreshToken>().AddAsync(newRefreshTokenEntity);
                await _unitOfWork.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

                return new ApiResponse<AuthResponseDto>
                {
                    Data = new AuthResponseDto
                    {
                        Token = jwtToken,
                        RefreshToken = newRefreshToken,
                        User = userDto,
                        ExpiresIn = _jwtSettings.ExpirationMinutes * 60
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "An error occurred during token refresh"
                };
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync(string refreshToken)
        {
            try
            {
                var refreshTokenEntity = await _unitOfWork.Repository<RefreshToken>()
                    .GetFirstAsync(rt => rt.Token == refreshToken);

                if (refreshTokenEntity != null)
                {
                    refreshTokenEntity.IsRevoked = true;
                    await _unitOfWork.SaveChangesAsync();
                }

                return new ApiResponse<bool> { Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred during logout"
                };
            }
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
