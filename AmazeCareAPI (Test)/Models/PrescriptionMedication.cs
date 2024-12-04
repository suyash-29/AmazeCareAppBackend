using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models
{
    public class PrescriptionMedication
    {
        [Key]
        public int PrescriptionMedicationID { get; set; }

        [ForeignKey("Prescription")]
        public int PrescriptionID { get; set; }

        [ForeignKey("Medication")]
        public int MedicationID { get; set; }

        public int Quantity { get; set; }

        public Prescription Prescription { get; set; }
        public Medication Medication { get; set; }
    }
}
