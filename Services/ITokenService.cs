using authDemo.Models;

namespace authDemo.Services
{
    public interface ITokenService
    {
        Task<string> GenerateJwtToken(User user);
        Task<string> GenerateRefreshToken(User model);
        Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(RefreshTokenModel model); 
    }
}
