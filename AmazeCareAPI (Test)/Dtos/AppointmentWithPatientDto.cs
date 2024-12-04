namespace AmazeCareAPI.Dtos
{
    public class AppointmentWithPatientDto
    {
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
        public string Symptoms { get; set; } 

        public string PatientName { get; set; }
        public string MedicalHistory { get; set; }
    }
}


