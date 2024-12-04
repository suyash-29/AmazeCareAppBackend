using AmazeCareAPI.Models;
using System;
using System.Threading.Tasks;

namespace AmazeCareAPI.Repositories.Interface
{
    public interface IUserRepository
    {
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<User> AddUserAsync(User user);
        Task<Patient> AddPatientAsync(Patient patient);

        Task<User> GetUserWithRoleAsync(string username);
    }
}
