using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models
{
    public class Medication
    {
        [Key]
        public int MedicationID { get; set; }

        [Required]
        [MaxLength(100)]
        public string MedicationName { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PricePerUnit { get; set; }

        public ICollection<PrescriptionMedication> PrescriptionMedications { get; set; }
    }
}
