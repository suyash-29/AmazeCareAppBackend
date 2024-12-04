using AmazeCareAPI.Data;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AmazeCareAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AmazeCareContext _context;

        public UserRepository(AmazeCareContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return !await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<User> AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Patient> AddPatientAsync(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<User> GetUserWithRoleAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role) 
                .FirstOrDefaultAsync(u => u.Username == username);
        }

    }
}
