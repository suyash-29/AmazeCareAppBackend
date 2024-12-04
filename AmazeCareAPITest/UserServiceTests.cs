using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using AmazeCareAPI.Dtos;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories.Interface;
using AmazeCareAPI.Services;

namespace AmazeCareAPITest
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private UserService _userService;

        [SetUp]
        public void SetUp()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _userService = new UserService(_mockUserRepository.Object);
        }

        [Test]
        public async Task CheckUsernameAvailabilityAsync_ShouldReturnAvailableMessage_WhenUsernameIsAvailable()
        {
            string username = "testuser";
            _mockUserRepository.Setup(repo => repo.IsUsernameAvailableAsync(username))
                .ReturnsAsync(true);

            var result = await _userService.CheckUsernameAvailabilityAsync(username);

            Assert.That(result.IsAvailable, Is.True);
            Assert.That(result.Message, Is.EqualTo("Username is available."));
            _mockUserRepository.Verify(repo => repo.IsUsernameAvailableAsync(username), Times.Once);
        }

        [Test]
        public async Task CheckUsernameAvailabilityAsync_ShouldReturnTakenMessage_WhenUsernameIsNotAvailable()
        {
            string username = "existinguser";
            _mockUserRepository.Setup(repo => repo.IsUsernameAvailableAsync(username))
                .ReturnsAsync(false);

            var result = await _userService.CheckUsernameAvailabilityAsync(username);

            Assert.That(result.IsAvailable, Is.False);
            Assert.That(result.Message, Is.EqualTo("Username is already taken. Please choose a different username."));
            _mockUserRepository.Verify(repo => repo.IsUsernameAvailableAsync(username), Times.Once);
        }

        [Test]
        public void CheckUsernameAvailabilityAsync_ShouldThrowServiceException_OnRepositoryException()
        {
            string username = "testuser";
            _mockUserRepository.Setup(repo => repo.IsUsernameAvailableAsync(username))
                .ThrowsAsync(new Exception("Database error"));

            var ex = Assert.ThrowsAsync<ServiceException>(async () =>
                await _userService.CheckUsernameAvailabilityAsync(username));
            Assert.That(ex.Message, Is.EqualTo("Error checking username availability."));
        }

        [Test]
        public async Task RegisterPatient_ShouldReturnCreatedPatient_WhenUserAndPatientAreValid()
        {
            var user = new User { UserID = 1, Username = "newuser" };
            var patient = new Patient
            {
                UserID = 1,
                FullName = "John Doe",
                Email = "johndoe@example.com",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male",
                ContactNumber = "1234567890",
                Address = "123 Main St",
                MedicalHistory = "None"
            };

            _mockUserRepository.Setup(repo => repo.AddUserAsync(user))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(repo => repo.AddPatientAsync(It.IsAny<Patient>()))
                .ReturnsAsync(patient);

            var result = await _userService.RegisterPatient(user, patient.FullName, patient.Email,
                patient.DateOfBirth, patient.Gender, patient.ContactNumber, patient.Address, patient.MedicalHistory);

            Assert.That(result.UserID, Is.EqualTo(patient.UserID));
            Assert.That(result.FullName, Is.EqualTo(patient.FullName));
            _mockUserRepository.Verify(repo => repo.AddUserAsync(user), Times.Once);
            _mockUserRepository.Verify(repo => repo.AddPatientAsync(It.IsAny<Patient>()), Times.Once);
        }

        [Test]
        public void RegisterPatient_ShouldThrowServiceException_OnRepositoryException()
        {
            var user = new User { UserID = 1, Username = "newuser" };

            _mockUserRepository.Setup(repo => repo.AddUserAsync(user))
                .ThrowsAsync(new Exception("Database error"));

            var ex = Assert.ThrowsAsync<ServiceException>(async () =>
                await _userService.RegisterPatient(user, "John Doe", "johndoe@example.com",
                    new DateTime(1990, 1, 1), "Male", "1234567890", "123 Main St", "None"));
            Assert.That(ex.Message, Is.EqualTo("Error registering patient."));
        }
    }
}
