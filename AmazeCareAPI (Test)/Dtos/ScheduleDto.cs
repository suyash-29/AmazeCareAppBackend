namespace AmazeCareAPI.Dtos
{
    public class ScheduleDto
    {
        public int ScheduleID { get; set; }
        public int DoctorID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}
