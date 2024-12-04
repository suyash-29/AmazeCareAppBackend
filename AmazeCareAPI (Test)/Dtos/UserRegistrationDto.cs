using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Dtos
{
    public class UserRegistrationDto
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public int RoleID { get; set; }
    }

}