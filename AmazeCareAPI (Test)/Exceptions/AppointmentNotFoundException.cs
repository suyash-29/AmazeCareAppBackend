namespace AmazeCareAPI.Exceptions
{
    public class AppointmentNotFoundException: Exception
    {
        public AppointmentNotFoundException(string message ) : base(message) { }
    }
}
