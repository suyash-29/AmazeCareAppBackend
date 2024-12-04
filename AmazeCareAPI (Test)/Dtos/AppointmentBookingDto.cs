namespace AmazeCareAPI.Dtos
{
    public class AppointmentBookingDto
    {
       
        public int DoctorID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Symptoms { get; set; }
    }

}
