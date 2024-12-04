namespace AmazeCareAPI.Dtos
{
    public class PatientDto
    {
        public int PatientID { get; set; } 
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public string MedicalHistory { get; set; }
    }
}
