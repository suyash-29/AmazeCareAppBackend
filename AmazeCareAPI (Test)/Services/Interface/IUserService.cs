using AmazeCareAPI.Dtos;
using AmazeCareAPI.Models;
using System;
using System.Threading.Tasks;

namespace AmazeCareAPI.Services.Interface
{
    public interface IUserService
    {
        Task<(bool IsAvailable, string Message)> CheckUsernameAvailabilityAsync(string username);
        Task<Patient> RegisterPatient(User user, string fullName, string email, DateTime dateOfBirth,
            string gender, string contactNumber, string address, string medicalHistory);
    }
}
