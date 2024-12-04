using AmazeCareAPI.Dtos;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories.Interface;
using AmazeCareAPI.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazeCareAPITest
{
    [TestFixture]
    public class PatientServiceTests
    {
        private Mock<IPatientRepository> _mockPatientRepository;
        private PatientService _patientService;

        [SetUp]
        public void SetUp()
        {
            _mockPatientRepository = new Mock<IPatientRepository>();
            _patientService = new PatientService(_mockPatientRepository.Object);
        }

        [Test]
        public async Task GetPersonalInfoAsync_ShouldReturnPatientPersonalInfo_WhenUserExists()
        {
            int userId = 1;
            var user = new User { UserID = userId, Username = "testUser" };
            var patient = new Patient
            {
                FullName = "Test User",
                Email = "testuser@example.com",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male",
                ContactNumber = "1234567890",
                Address = "123 Test St, Test City",
                MedicalHistory = "No significant medical history."
            };

            _mockPatientRepository.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockPatientRepository.Setup(repo => repo.GetPatientByUserIdAsync(userId)).ReturnsAsync(patient);

            var result = await _patientService.GetPersonalInfoAsync(userId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.Username, Is.EqualTo(user.Username));
            Assert.That(result.FullName, Is.EqualTo(patient.FullName));
            Assert.That(result.Email, Is.EqualTo(patient.Email));
            Assert.That(result.DateOfBirth, Is.EqualTo(patient.DateOfBirth));
            Assert.That(result.Gender, Is.EqualTo(patient.Gender));
            Assert.That(result.ContactNumber, Is.EqualTo(patient.ContactNumber));
            Assert.That(result.Address, Is.EqualTo(patient.Address));
            Assert.That(result.MedicalHistory, Is.EqualTo(patient.MedicalHistory));
        }

        [Test]
        public async Task UpdatePersonalInfoAsync_ShouldReturnSuccess_WhenPersonalInfoIsUpdated()
        {
            int userId = 1;
            var updateDto = new UpdatePersonalInfoDto
            {
                Username = "updatedUsername",
                NewPassword = "newPassword123",
                FullName = "Updated Name",
                Email = "updated@example.com",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Female",
                ContactNumber = "0987654321",
                Address = "456 Updated St, Updated City",
                MedicalHistory = "Updated medical history"
            };

            var user = new User { UserID = userId, Username = "oldUsername", PasswordHash = "oldHash" };
            var patient = new Patient
            {
                FullName = "Old Name",
                Email = "old@example.com",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male",
                ContactNumber = "1234567890",
                Address = "123 Old St, Old City",
                MedicalHistory = "Old medical history"
            };

            _mockPatientRepository.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockPatientRepository.Setup(repo => repo.GetPatientByUserIdAsync(userId)).ReturnsAsync(patient);
            _mockPatientRepository.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockPatientRepository.Setup(repo => repo.UpdatePatientAsync(It.IsAny<Patient>())).Returns(Task.CompletedTask);
            _mockPatientRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);
            _mockPatientRepository.Setup(repo => repo.IsUsernameAvailableAsync(updateDto.Username))
                .ReturnsAsync(true);

            var result = await _patientService.UpdatePersonalInfoAsync(userId, updateDto);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Personal information updated successfully."));
            Assert.That(user.Username, Is.EqualTo(updateDto.Username));

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(updateDto.NewPassword, user.PasswordHash);
            Assert.That(isPasswordValid, Is.True);

            Assert.That(patient.FullName, Is.EqualTo(updateDto.FullName));
            Assert.That(patient.Email, Is.EqualTo(updateDto.Email));
            Assert.That(patient.ContactNumber, Is.EqualTo(updateDto.ContactNumber));
            Assert.That(patient.Address, Is.EqualTo(updateDto.Address));
            Assert.That(patient.MedicalHistory, Is.EqualTo(updateDto.MedicalHistory));
            Assert.That(patient.DateOfBirth, Is.EqualTo(updateDto.DateOfBirth));
            Assert.That(patient.Gender, Is.EqualTo(updateDto.Gender));
        }

        [Test]
        public async Task UpdatePersonalInfoAsync_ShouldReturnFailure_WhenUsernameIsTaken()
        {
            int userId = 1;
            var updateDto = new UpdatePersonalInfoDto
            {
                Username = "takenUsername",
                NewPassword = "newPassword123",
                FullName = "Updated Name",
                Email = "updated@example.com",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Female",
                ContactNumber = "0987654321",
                Address = "456 Updated St, Updated City",
                MedicalHistory = "Updated medical history"
            };

            var existingUser = new User { UserID = 2, Username = "takenUsername" };

            _mockPatientRepository.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(new User { UserID = userId, Username = "oldUsername" });
            _mockPatientRepository.Setup(repo => repo.GetPatientByUserIdAsync(userId)).ReturnsAsync(new Patient());
            _mockPatientRepository.Setup(repo => repo.IsUsernameAvailableAsync(updateDto.Username))
                .ReturnsAsync(false);

            var result = await _patientService.UpdatePersonalInfoAsync(userId, updateDto);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("Username is already taken. Please choose a different username."));
        }

        [Test]
        public async Task SearchDoctors_ShouldReturnDoctors_WhenSpecializationIsProvided()
        {
            string specialization = "Cardiologist";
            var doctors = new List<DoctorDto>
            {
                new DoctorDto
                {
                    DoctorID = 1,
                    FullName = "Dr. John Doe",
                    ExperienceYears = 10,
                    Qualification = "MBBS",
                    Designation = "Active",
                    Email = "johndoe@example.com",
                    Specializations = new List<string> { "Cardiologist" }
                },
                new DoctorDto
                {
                    DoctorID = 2,
                    FullName = "Dr. Jane Smith",
                    ExperienceYears = 8,
                    Qualification = "MD",
                    Designation = "Active",
                    Email = "janesmith@example.com",
                    Specializations = new List<string> { "Cardiologist" }
                }
            };

            _mockPatientRepository.Setup(repo => repo.SearchDoctorsAsync(specialization))
                .ReturnsAsync(doctors);

            var result = await _patientService.SearchDoctors(specialization);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().FullName, Is.EqualTo("Dr. John Doe"));
            Assert.That(result.All(d => d.Specializations.Contains(specialization)), Is.True);
        }

        [Test]
        public async Task SearchDoctors_ShouldReturnDoctors_WhenNoSpecializationIsProvided()
        {
            var doctors = new List<DoctorDto>
            {
                new DoctorDto
                {
                    DoctorID = 1,
                    FullName = "Dr. John Doe",
                    ExperienceYears = 10,
                    Qualification = "MBBS",
                    Designation = "Active",
                    Email = "johndoe@example.com",
                    Specializations = new List<string> { "Cardiologist" }
                },
                new DoctorDto
                {
                    DoctorID = 2,
                    FullName = "Dr. Jane Smith",
                    ExperienceYears = 8,
                    Qualification = "MD",
                    Designation = "Active",
                    Email = "janesmith@example.com",
                    Specializations = new List<string> { "Neurologist" }
                }
            };

            _mockPatientRepository.Setup(repo => repo.SearchDoctorsAsync(null))
                .ReturnsAsync(doctors);

            var result = await _patientService.SearchDoctors(null);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(d => d.Designation != "Inactive"), Is.True);
        }
    }
}
