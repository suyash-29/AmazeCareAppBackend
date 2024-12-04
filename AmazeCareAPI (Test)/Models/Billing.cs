using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models
{
    public class Billing
    {
        [Key]
        public int BillingID { get; set; }

        [ForeignKey("Patient")]
        public int PatientID { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorID { get; set; }

        [ForeignKey("MedicalRecord")]
        public int MedicalRecordID { get; set; }

       

        [Column(TypeName = "decimal(10, 2)")]
        public decimal ConsultationFee { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalTestsPrice { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalMedicationsPrice { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal GrandTotal { get;  set; }  // Calculated in C#

        [MaxLength(10)]
        public string Status { get; set; } = "Pending";

        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public MedicalRecord MedicalRecord { get; set; }
        public Prescription Prescription { get; set; }

        // Method to calculate GrandTotal
        public void CalculateGrandTotal()
        {
            GrandTotal = ConsultationFee + TotalTestsPrice + TotalMedicationsPrice;
        }
    }
}
