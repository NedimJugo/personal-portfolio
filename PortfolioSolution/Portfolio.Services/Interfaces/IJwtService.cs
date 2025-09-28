using Portfolio.Services.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateJwtToken(ApplicationUser user, IList<string> roles);
        RefreshToken GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        Task<bool> ValidateRefreshTokenAsync(string token, int userId);
        Task RevokeRefreshTokenAsync(string token);
        Task RevokeAllUserRefreshTokensAsync(int userId);
        Task CleanupExpiredRefreshTokensAsync();
    }
}
