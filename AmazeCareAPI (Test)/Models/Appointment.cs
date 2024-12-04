using System;
using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models
{

    public class Appointment
    {
        [Key]
        public int AppointmentID { get; set; }

        public int PatientID { get; set; }

        public int DoctorID { get; set; }

        public DateTime AppointmentDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        public string Symptoms { get; set; }

        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }

        public MedicalRecord? MedicalRecord { get; set; }

       
    }

}
