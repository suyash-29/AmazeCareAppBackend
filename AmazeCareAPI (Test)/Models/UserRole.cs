using System.ComponentModel.DataAnnotations;


namespace AmazeCareAPI.Models
{
    public class UserRole
    {
        [Key]
        public int RoleID { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; }
    }
}
