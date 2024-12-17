using authDemo.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace authDemo.Services
{
    public class UserService : IUserService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
       

        public UserService(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, 
                            SignInManager<User> signInManager, ITokenService tokenService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }


        public async Task<(string AccessToken, string RefreshToken)> LoginAsync(LoginModel model)
        {
            User user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return (null,null);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

            if (result.Succeeded)
            {
                string accesToken = await _tokenService.GenerateJwtToken(user);
                string reFreshtoken = await _tokenService.GenerateRefreshToken(user);

                return (accesToken, reFreshtoken);
            }

            return (null,null);
        }

        public async Task<IdentityResult> RegisterAsync(RegisterModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                return IdentityResult.Failed(new IdentityError { Description = "A jelszavak nem egyeznek"});
            }

            User user = new User 
            {
                UserName = model.Email,
                FullName = model.FullName,
                Email = model.Email,
            };

            

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (model.Email == "admin@admin.hu")
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
                
            }
            
            return result;
        }

        public async Task<IdentityResult> DeleteAsync(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Nem létezik a felhasználó" });
            }

            var result = await _userManager.DeleteAsync(user);

            return result;
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, PasswordModel model)
        {
            User? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError{Description = "A felhasználó nem található"});
            }

            bool passwordValid = await _userManager.CheckPasswordAsync(user, model.OldPassword);

            if (!passwordValid)
            {
                return IdentityResult.Failed(new IdentityError { Description = "A régi jelszó nem megfelelő" });
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Hibás jelszó ellenőrzés" });
            }

            IdentityResult result =  await _userManager.ChangePasswordAsync(user,model.OldPassword, model.NewPassword);

            return result;
        }
    }
}
