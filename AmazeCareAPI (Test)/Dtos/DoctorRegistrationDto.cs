namespace AmazeCareAPI.Dtos
{
    public class DoctorRegistrationDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int? ExperienceYears { get; set; }
        public string Qualification { get; set; }
        public string Designation { get; set; }
        public List<int> SpecializationIds { get; set; }
    }
}
