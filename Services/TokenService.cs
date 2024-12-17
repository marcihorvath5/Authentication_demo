using authDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace authDemo.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly UserDb _userDb;


        public TokenService(IConfiguration configuration, UserManager<User> userManager, UserDb userDb)
        {
            _configuration = configuration;
            _userManager = userManager;
            _userDb = userDb;
        }

        public async Task<string> GenerateJwtToken(User user)
        {
            var claim = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claim.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtSettings = _configuration.GetSection("Jwt");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claim,
                expires: DateTime.UtcNow.AddMinutes(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken(User model)
        {
            RefreshToken refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                CreateTime = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(5),
                UserId = model.Id,
                User = model,
            };

            _userDb.RefreshTokens.Add(refreshToken);

            await _userDb.SaveChangesAsync();

            return refreshToken.Token;
        }

        public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(RefreshTokenModel model)
        {
            RefreshToken? storedToken = await _userDb.RefreshTokens
                                                    .Include(u => u.User)
                                                    .FirstOrDefaultAsync(t => t.Token == model.Token);

            if (storedToken == null || storedToken.IsUsed == true || storedToken.IsRevoked == true || storedToken.Expires < DateTime.UtcNow)
            {
                throw new Exception("Érvénytelen refreshtoken");
            }

            if (storedToken.UserId != model.UserId)
            {
                throw new Exception("Érvénytelen felhasználó");
            }

            storedToken.IsUsed = true;

            string newAccessToken = await GenerateJwtToken(storedToken.User);
            string newRefreshToken = await GenerateRefreshToken(storedToken.User);

            await _userDb.SaveChangesAsync();

            return (newAccessToken, newRefreshToken);
        }
    }
}
