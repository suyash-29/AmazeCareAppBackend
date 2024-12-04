namespace AmazeCareAPI.Dtos
{
    public class ScheduleWithDoctorDto
    {
        public int ScheduleID { get; set; }
        public int DoctorID { get; set; }
        public string DoctorName { get; set; } // Include doctor's name
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}
