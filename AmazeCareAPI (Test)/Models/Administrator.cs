using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models
{
    public class Administrator
    {
        [Key]
        public int AdminID { get; set; }

        public int? UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        public string Email { get; set; }


        public User User { get; set; }
    }
}