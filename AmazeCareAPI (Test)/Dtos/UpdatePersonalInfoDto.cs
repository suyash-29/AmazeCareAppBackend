﻿namespace AmazeCareAPI.Dtos
{
    public class UpdatePersonalInfoDto
    {
        public string Username { get; set; }
        public string? NewPassword { get; set; } 
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public string MedicalHistory { get; set; }
    }
}
