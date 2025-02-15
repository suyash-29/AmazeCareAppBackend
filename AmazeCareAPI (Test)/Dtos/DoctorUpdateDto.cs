﻿namespace AmazeCareAPI.Dtos
{
    public class DoctorUpdateDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public int? ExperienceYears { get; set; }
        public string Qualification { get; set; }
        public string Designation { get; set; }
        public List<int> SpecializationIds { get; set; }
    }
}
