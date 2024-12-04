using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories.Interface;
using AmazeCareAPI.Services;
using System.Data;
using System.Security.Claims;

namespace AmazeCareAPITest
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IConfiguration> _mockConfiguration;
        private AuthenticationService _authenticationService;

        [SetUp]
        public void SetUp()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(config => config["Jwt:Key"]).Returns("YourSecretKeyHere123456789012345678901234567890");
            _mockConfiguration.Setup(config => config["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(config => config["Jwt:Audience"]).Returns("TestAudience");

            _authenticationService = new AuthenticationService(_mockUserRepository.Object, _mockConfiguration.Object);
        }

        [Test]
        public async Task AuthenticateUser_ShouldReturnToken_WhenCredentialsAreValid()
        {
            string username = "admin";
            string password = "admin";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var userRole = new UserRole
            {
                RoleID = 1,
                RoleName = "Patient"
            };

            var user = new User
            {
                UserID = 3,
                Username = username,
                PasswordHash = hashedPassword,
                RoleID = 1,
                Role = userRole 
            };

            _mockUserRepository.Setup(repo => repo.GetUserWithRoleAsync(username))
                .ReturnsAsync(user);

            var token = await _authenticationService.AuthenticateUser(username, password);

            Assert.That(token, Is.Not.Null);
            Assert.That(token, Is.InstanceOf<string>());

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            Assert.That(jwtToken.Issuer, Is.EqualTo("TestIssuer"));
            Assert.That(jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value, Is.EqualTo("Patient"));

            _mockUserRepository.Verify(repo => repo.GetUserWithRoleAsync(username), Times.Once);
        }

        [Test]
        public void AuthenticateUser_ShouldThrowAuthenticationException_WhenUserNotFound()
        {
            string username = "unknownuser";
            string password = "Password123";

            _mockUserRepository.Setup(repo => repo.GetUserWithRoleAsync(username))
                .ReturnsAsync((User)null);

            var ex = Assert.ThrowsAsync<AuthenticationException>(async () =>
                await _authenticationService.AuthenticateUser(username, password));

            Assert.That(ex.Message, Is.EqualTo("Invalid username or password."));
        }

        [Test]
        public void AuthenticateUser_ShouldThrowAuthenticationException_WhenPasswordIsInvalid()
        {
            string username = "testuser";
            string password = "WrongPassword";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");

            var user = new User
            {
                UserID = 1,
                Username = username,
                PasswordHash = hashedPassword,
            };

            _mockUserRepository.Setup(repo => repo.GetUserWithRoleAsync(username))
                .ReturnsAsync(user);

            var ex = Assert.ThrowsAsync<AuthenticationException>(async () =>
                await _authenticationService.AuthenticateUser(username, password));

            Assert.That(ex.Message, Is.EqualTo("Invalid username or password."));
        }

        [Test]
        public void AuthenticateUser_ShouldThrowServiceException_OnUnexpectedException()
        {
            string username = "testuser";
            string password = "Password123";

            _mockUserRepository.Setup(repo => repo.GetUserWithRoleAsync(username))
                .ThrowsAsync(new Exception("Database error"));

            var ex = Assert.ThrowsAsync<ServiceException>(async () =>
                await _authenticationService.AuthenticateUser(username, password));

            Assert.That(ex.Message, Is.EqualTo("An error occurred during authentication."));
        }
    }
}
