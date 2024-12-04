namespace AmazeCareAPI.Dtos
{
    public class PatientTestDetailDto
    {
        public int AppointmentId { get; set; }
        public string DoctorName { get; set; }
        public int TestId { get; set; }
        public string TestName { get; set; }
        public decimal TestPrice { get; set; }
    }

}
