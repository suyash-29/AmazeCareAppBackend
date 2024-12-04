namespace AmazeCareAPI.Dtos
{
    public class PatientPrescriptionDetailDto
    {
        public int AppointmentId { get; set; }
        public string DoctorName { get; set; }
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public int DurationDays { get; set; }
        public int Quantity { get; set; }
    }
}
