using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmazeCareAPI.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionID { get; set; }

        [ForeignKey("MedicalRecord")]
        public int RecordID { get; set; }

        public int MedicationID { get; set; }

        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public int DurationDays { get; set; }

        public int Quantity { get; set; } // New property

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalPrice { get; set; }  // Calculated in C#

        public MedicalRecord MedicalRecord { get; set; }
        public ICollection<PrescriptionMedication> PrescriptionMedications { get; set; }

        [ForeignKey("Billing")]
        public int? BillingID { get; set; }  // Nullable foreign key to Billing
        public Billing Billing { get; set; }

        // Method to calculate the total price of medications
        public void CalculateTotalPrice()
        {
            TotalPrice = 0;
            if (PrescriptionMedications != null)
            {
                foreach (var prescriptionMedication in PrescriptionMedications)
                {
                    TotalPrice += prescriptionMedication.Quantity * prescriptionMedication.Medication.PricePerUnit;
                }
            }
        }
    }
}
