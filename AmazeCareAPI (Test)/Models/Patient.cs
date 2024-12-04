using System;
using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models
{

    public class Patient
    {
        [Key]
        public int PatientID { get; set; }

        public int? UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        public DateTime DateOfBirth { get; set; }

        [MaxLength(10)]
        public string Gender { get; set; }

        [MaxLength(15)]
        public string ContactNumber { get; set; }

        public string Email { get; set; }


        [MaxLength(200)]
        public string Address { get; set; }

        public string MedicalHistory { get; set; }

        public User User { get; set; }

        public ICollection<Billing> Billings { get; set; }
    }

}
