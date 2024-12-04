using System;
using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models
{



    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public int RoleID { get; set; }


        public UserRole Role { get; set; }
    }
}
