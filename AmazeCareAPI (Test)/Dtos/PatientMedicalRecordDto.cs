namespace AmazeCareAPI.Dtos
{
    public class PatientMedicalRecordDto
    {
        public int? MedicalRecordID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string DoctorName { get; set; }
        public string Symptoms { get; set; }
        public string PhysicalExamination { get; set; }
        public string TreatmentPlan { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public List<TestDto> Tests { get; set; }
        public List<PrescriptionDto> Prescriptions { get; set; }
        public BillingDto BillingDetails { get; set; }
    }
}
