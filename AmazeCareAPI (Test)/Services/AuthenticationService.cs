using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AmazeCareAPI.Services
{
    public class AuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthenticationService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<string> AuthenticateUser(string username, string password)
        {
            try
            {
                var user = await _userRepository.GetUserWithRoleAsync(username);

                if (user == null || !VerifyPassword(password, user.PasswordHash))
                {
                    throw new AuthenticationException("Invalid username or password.");
                }

                return GenerateJwtToken(user);
            }
            catch (Exception ex) when (!(ex is AuthenticationException))
            {
                throw new ServiceException("An error occurred during authentication.", ex);
            }
        }


        private bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
        private string GenerateJwtToken(User user)
        {
            try
            {
                var role = user.Role?.RoleName ?? "User";

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, role)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error generating JWT token.", ex);
            }
        }
    }

}
