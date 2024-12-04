namespace AmazeCareAPI.Dtos
{
    public class DoctorDto
    {
        public int? DoctorID { get; set; } // Null for new doctor
        public string FullName { get; set; }
        public string Email { get; set; }
        public int? ExperienceYears { get; set; }
        public string Qualification { get; set; }
        public string Designation { get; set; }
        public List<string> Specializations { get; set; }

       
    }
}
