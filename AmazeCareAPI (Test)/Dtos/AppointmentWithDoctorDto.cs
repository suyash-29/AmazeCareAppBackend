using System.Globalization;

namespace AmazeCareAPI.Dtos
{
    public class AppointmentWithDoctorDto
    {
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public int DoctorID { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }

        public string Status { get; set; }
    }

}
