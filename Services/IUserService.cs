using authDemo.Models;
using Microsoft.AspNetCore.Identity;

namespace authDemo.Services
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterAsync(RegisterModel model);
        Task<(string AccessToken, string RefreshToken)> LoginAsync(LoginModel model);
        Task<IdentityResult> DeleteAsync(string email);
        Task<IdentityResult> ChangePasswordAsync(string userId, PasswordModel model);
        Task<bool> SaveFile(FileModel model); 
    }
}
