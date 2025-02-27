﻿namespace authDemo.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsUsed { get; set; } = false;
        public bool IsRevoked { get; set; } = false;
        public DateTime CreateTime { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}
