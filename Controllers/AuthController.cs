using authDemo.Models;
using authDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace authDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _iTokenService;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;

        public AuthController(ITokenService tokenService, UserManager<User> userManager, IUserService userService)
        {
            _iTokenService = tokenService;
            _userManager = userManager;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {  return BadRequest(ModelState); }

            var result = await _userService.RegisterAsync(model);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Felhasználó regisztrálva");
        }

        [HttpPost]
        public async Task<IActionResult> LogInAsync([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tokens = await  _userService.LoginAsync(model);

            if (tokens.AccessToken == null || tokens.RefreshToken == null)
            {
                return BadRequest("Érvénytelen felhasználó név vagy jelszó");
            }

            return Ok(new 
            {AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken});
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletAsync(string email)
        {
            IdentityResult result = await _userService.DeleteAsync(email);

            if (result.Succeeded)
            {
                return Ok("Felhasználó eltávolítva");
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordModel model) 
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest("Érvénytelen adatok");
            }

            string userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            IdentityResult result = await _userService.ChangePasswordAsync(userId, model);
            if (result.Succeeded) 
            {
                return Ok("Jelszó frissítve");
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenRequest([FromBody]  RefreshTokenModel model) 
        {
            var tokens = await _iTokenService.RefreshTokenAsync(model);

            try
            {
                return Ok(new
                {
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                });
            }
            catch (Exception ex)
            { 
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
