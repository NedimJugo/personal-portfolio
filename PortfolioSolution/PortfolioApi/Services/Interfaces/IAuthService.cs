using PortfolioApi.Models.DTOs;

namespace PortfolioApi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<ApiResponse<bool>> LogoutAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string token);
    }
}
