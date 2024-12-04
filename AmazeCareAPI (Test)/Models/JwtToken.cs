using System;
using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models
{

    public class JwtToken
    {
        [Key]
        public int TokenID { get; set; }

        public int UserID { get; set; }

        [Required]
        public string Token { get; set; }

        public DateTime ExpirationDate { get; set; }

        public bool IsRevoked { get; set; } = false;

        public User User { get; set; }
    }
}
