using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models
{
    public class Specialization
    {
        [Key]
        public int SpecializationID { get; set; }

        [Required]
        [MaxLength(100)]
        public string SpecializationName { get; set; }

        public ICollection<DoctorSpecialization> DoctorSpecializations { get; set; }
    }

}