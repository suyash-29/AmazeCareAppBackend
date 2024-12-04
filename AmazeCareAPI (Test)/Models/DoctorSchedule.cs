using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models
{
    public class DoctorSchedule
    {
        [Key]
        public int ScheduleID { get; set; }
        public int DoctorID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Scheduled";

        public Doctor Doctor { get; set; }
    }

}
