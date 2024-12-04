using AmazeCareAPI.Dtos;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories.Interface;
using AmazeCareAPI.Services.Interface;
using System;
using System.Threading.Tasks;

namespace AmazeCareAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<(bool IsAvailable, string Message)> CheckUsernameAvailabilityAsync(string username)
        {
            try
            {
                bool isAvailable = await _userRepository.IsUsernameAvailableAsync(username);
                string message = isAvailable
                    ? "Username is available."
                    : "Username is already taken. Please choose a different username.";

                return (isAvailable, message);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error checking username availability.", ex);
            }
        }
        public async Task<Patient> RegisterPatient(User user, string fullName, string email, DateTime dateOfBirth,
            string gender, string contactNumber, string address, string medicalHistory)
        {
            try
            {
                user.RoleID = 1;
                var createdUser = await _userRepository.AddUserAsync(user);

                var patient = new Patient
                {
                    UserID = createdUser.UserID,
                    FullName = fullName,
                    Email = email,
                    DateOfBirth = dateOfBirth,
                    Gender = gender,
                    ContactNumber = contactNumber,
                    Address = address,
                    MedicalHistory = medicalHistory
                };

                return await _userRepository.AddPatientAsync(patient);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error registering patient.", ex);
            }
        }
    }
}

