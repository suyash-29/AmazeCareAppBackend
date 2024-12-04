namespace AmazeCareAPI.Dtos
{
    public class UpdateScheduleDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}
