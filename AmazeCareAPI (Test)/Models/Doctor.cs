using System.ComponentModel.DataAnnotations;


namespace AmazeCareAPI.Models
{

    public class Doctor
    {
        [Key]
        public int DoctorID { get; set; }

        public int? UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }
        public string Email { get; set; }


        public int? ExperienceYears { get; set; }

        [MaxLength(100)]
        public string Qualification { get; set; }

        [MaxLength(50)]
        public string Designation { get; set; }

        public User User { get; set; }
        public ICollection<DoctorSpecialization> DoctorSpecializations { get; set; }

        public ICollection<Billing> Billings { get; set; }

        public ICollection<DoctorSchedule> DoctorSchedule { get; set; }

    }

}