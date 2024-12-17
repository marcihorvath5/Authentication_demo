using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace authDemo.Models
{
    public class User:IdentityUser
    {
        public string FullName { get; set; }
        public string? Gender { get; set; }
        public string? Country { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
