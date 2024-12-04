using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Dtos
{
    public class PrescriptionCreateDto
    {
        [Required]
        public int RecordID { get; set; }

        [Required]
        [MaxLength(100)]
        public string MedicationName { get; set; }

        [MaxLength(20)]
        public string Dosage { get; set; }

        public int DurationDays { get; set; }
    }
}
