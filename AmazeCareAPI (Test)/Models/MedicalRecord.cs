using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmazeCareAPI.Models
{
    public class MedicalRecord
    {
        [Key]
        public int RecordID { get; set; }

        [ForeignKey("Appointment")]
        public int AppointmentID { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorID { get; set; }

        [ForeignKey("Patient")]
        public int PatientID { get; set; }

        public string Symptoms { get; set; }
        public string PhysicalExamination { get; set; }
        public string TreatmentPlan { get; set; }
        public DateTime? FollowUpDate { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalPrice { get; set; } 

        public Appointment Appointment { get; set; }
        public Doctor Doctor { get; set; }
        public Patient Patient { get; set; }
        public ICollection<MedicalRecordTest> MedicalRecordTests { get; set; }



        [ForeignKey("Billing")]
        public int? BillingID { get; set; }  
        public Billing Billing { get; set; }

        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    

    // Method to calculate the total price of tests
    public void CalculateTotalPrice()
        {
            TotalPrice = 0;
            if (MedicalRecordTests != null)
            {
                foreach (var recordTest in MedicalRecordTests)
                {
                    TotalPrice += recordTest.Test.TestPrice;
                }
            }
        }
    }
}
