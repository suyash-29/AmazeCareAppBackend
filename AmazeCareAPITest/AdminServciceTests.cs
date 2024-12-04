using AmazeCareAPI.Dtos;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories;
using AmazeCareAPI.Repositories.Interface;
using AmazeCareAPI.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazeCareAPITest
{
    [TestFixture]
    public class AdminServiceTests
    {
        private Mock<IAdminRepository> _mockAdminRepository;
        private AdminService _adminService;

        [SetUp]
        public void Setup()
        {
            _mockAdminRepository = new Mock<IAdminRepository>();
            _adminService = new AdminService(_mockAdminRepository.Object);
        }
        [Test]
        public async Task RegisterAdmin_WhenUsernameIsAvailable_ReturnsAdmin()
        {
            var username = "adminUser";
            var password = "password123";
            var fullName = "Admin User";
            var email = "admin@example.com";

            var user = new User
            {
                UserID = 1,
                Username = username,
                PasswordHash = "hashedPassword",
                RoleID = 3
            };

            var admin = new Administrator
            {
                UserID = user.UserID,
                FullName = fullName,
                Email = email
            };

            _mockAdminRepository.Setup(repo => repo.IsUsernameAvailableAsync(username)).ReturnsAsync(true);
            _mockAdminRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) =>
                {
                    user.UserID = 1;
                    return user;
                });

            _mockAdminRepository.Setup(repo => repo.CreateAdminAsync(It.IsAny<Administrator>()))
                .ReturnsAsync((Administrator admin) =>
                {
                    admin.UserID = 1;
                    return admin;
                });

            var result = await _adminService.RegisterAdmin(username, password, fullName, email);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserID, Is.EqualTo(1));
            Assert.That(result.FullName, Is.EqualTo(fullName));
            Assert.That(result.Email, Is.EqualTo(email));
        }

        [Test]
        public async Task RegisterDoctor_WhenUsernameIsAvailable_ReturnsDoctor()
        {
            var doctorDto = new DoctorRegistrationDto
            {
                Username = "doctorUser",
                Password = "password123",
                FullName = "Doctor User",
                Email = "doctor@example.com",
                ExperienceYears = 5,
                Qualification = "MBBS",
                Designation = "Consultant",
                SpecializationIds = new List<int> { 1, 2 }
            };

            var user = new User
            {
                UserID = 1,
                Username = doctorDto.Username,
                PasswordHash = "hashedPassword",
                RoleID = 2
            };

            var doctor = new Doctor
            {
                UserID = user.UserID,
                FullName = doctorDto.FullName,
                Email = doctorDto.Email,
                ExperienceYears = doctorDto.ExperienceYears,
                Qualification = doctorDto.Qualification,
                Designation = doctorDto.Designation
            };

            _mockAdminRepository.Setup(repo => repo.IsUsernameAvailableAsync(doctorDto.Username)).ReturnsAsync(true);
            _mockAdminRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) =>
                {
                    user.UserID = 1;
                    return user;
                });

            _mockAdminRepository.Setup(repo => repo.CreateDoctorAsync(It.IsAny<Doctor>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync((Doctor doctor, IEnumerable<int> _) =>
                {
                    doctor.UserID = 1;
                    return doctor;
                });

            var result = await _adminService.RegisterDoctor(doctorDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserID, Is.EqualTo(1));
            Assert.That(result.FullName, Is.EqualTo(doctorDto.FullName));
            Assert.That(result.Email, Is.EqualTo(doctorDto.Email));
        }

        [Test]
        public async Task UpdateDoctorDetails_WhenDoctorExists_UpdatesDoctorDetailsSuccessfully()
        {
            var doctorId = 1;
            var doctorDto = new DoctorUpdateDto
            {
                FullName = "Updated Doctor Name",
                Email = "updated@example.com",
                ExperienceYears = 10,
                Qualification = "MD",
                Designation = "Senior Consultant",
                SpecializationIds = new List<int> { 1, 2 }
            };

            var doctor = new Doctor
            {
                DoctorID = doctorId,
                FullName = "Doctor Name",
                Email = "doctor@example.com",
                ExperienceYears = 5,
                Qualification = "MBBS",
                Designation = "Consultant",
                DoctorSpecializations = new List<DoctorSpecialization> { new DoctorSpecialization { SpecializationID = 1 } }
            };

            _mockAdminRepository.Setup(repo => repo.GetDoctorWithSpecializationsAsync(doctorId))
                .ReturnsAsync(doctor);
            _mockAdminRepository.Setup(repo => repo.UpdateDoctorSpecializationsAsync(doctorId, doctorDto.SpecializationIds))
                .Returns(Task.CompletedTask);
            _mockAdminRepository.Setup(repo => repo.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _adminService.UpdateDoctorDetails(doctorId, doctorDto);

            Assert.That(result, Is.True);
            Assert.That(doctor.FullName, Is.EqualTo(doctorDto.FullName));
            Assert.That(doctor.Email, Is.EqualTo(doctorDto.Email));
            Assert.That(doctor.ExperienceYears, Is.EqualTo(doctorDto.ExperienceYears));
            Assert.That(doctor.Qualification, Is.EqualTo(doctorDto.Qualification));
            Assert.That(doctor.Designation, Is.EqualTo(doctorDto.Designation));
        }

        [Test]
        public async Task UpdateDoctorDetails_WhenDoctorDoesNotExist_ReturnsFalse()
        {
            var doctorId = 1;
            var doctorDto = new DoctorUpdateDto
            {
                FullName = "Updated Doctor Name",
                Email = "updated@example.com"
            };

            _mockAdminRepository.Setup(repo => repo.GetDoctorWithSpecializationsAsync(doctorId))
                .ReturnsAsync((Doctor)null);

            var result = await _adminService.UpdateDoctorDetails(doctorId, doctorDto);

            Assert.That(result, Is.False);
        }
        [Test]
        public async Task DeleteDoctor_WhenDoctorExists_DeletesDoctorSuccessfully()
        {
            var userId = 1;
            var doctorId = 1;

            var doctor = new Doctor
            {
                DoctorID = doctorId,
                UserID = userId,
                Designation = "Consultant"
            };

            var scheduledAppointments = new List<Appointment>
    {
        new Appointment { AppointmentID = 1, Status = "Scheduled" },
        new Appointment { AppointmentID = 2, Status = "Scheduled" }
    };

            _mockAdminRepository.Setup(repo => repo.GetDoctorByIdAndUserIdAsync(doctorId, userId))
                .ReturnsAsync(doctor);
            _mockAdminRepository.Setup(repo => repo.GetScheduledAppointmentsAsync(doctorId))
                .ReturnsAsync(scheduledAppointments);
            _mockAdminRepository.Setup(repo => repo.DeleteUserAsync(userId))
                .Returns(Task.CompletedTask);
            _mockAdminRepository.Setup(repo => repo.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _adminService.DeleteDoctor(userId, doctorId);

            Assert.That(result, Is.True);
            Assert.That(doctor.UserID, Is.Null);
            Assert.That(doctor.Designation, Is.EqualTo("Inactive"));
            Assert.That(scheduledAppointments[0].Status, Is.EqualTo("Canceled"));
            Assert.That(scheduledAppointments[1].Status, Is.EqualTo("Canceled"));
            _mockAdminRepository.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
            _mockAdminRepository.Verify(repo => repo.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteDoctor_WhenDoctorAlreadyInactive_DoesNotModifyDesignation()
        {
            var userId = 1;
            var doctorId = 1;

            var doctor = new Doctor
            {
                DoctorID = doctorId,
                UserID = userId,
                Designation = "Inactive"
            };

            _mockAdminRepository.Setup(repo => repo.GetDoctorByIdAndUserIdAsync(doctorId, userId))
                .ReturnsAsync(doctor);
            _mockAdminRepository.Setup(repo => repo.GetScheduledAppointmentsAsync(doctorId))
                .ReturnsAsync(new List<Appointment>());
            _mockAdminRepository.Setup(repo => repo.DeleteUserAsync(userId))
                .Returns(Task.CompletedTask);
            _mockAdminRepository.Setup(repo => repo.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _adminService.DeleteDoctor(userId, doctorId);

            Assert.That(result, Is.True);
            Assert.That(doctor.Designation, Is.EqualTo("Inactive"));
            _mockAdminRepository.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
            _mockAdminRepository.Verify(repo => repo.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task GetDoctorDetails_WhenDoctorExists_ReturnsDoctorDetails()
        {
            var doctorId = 1;

            var doctor = new Doctor
            {
                DoctorID = doctorId,
                FullName = "Dr. John Doe",
                Email = "doctor@example.com",
                ExperienceYears = 10,
                Qualification = "MBBS",
                Designation = "Consultant"
            };

            _mockAdminRepository.Setup(repo => repo.GetDoctorByIdAsync(doctorId))
                .ReturnsAsync(doctor);

            var result = await _adminService.GetDoctorDetails(doctorId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DoctorID, Is.EqualTo(doctorId));
            Assert.That(result.FullName, Is.EqualTo("Dr. John Doe"));
            Assert.That(result.Email, Is.EqualTo("doctor@example.com"));
            Assert.That(result.ExperienceYears, Is.EqualTo(10));
            Assert.That(result.Qualification, Is.EqualTo("MBBS"));
            Assert.That(result.Designation, Is.EqualTo("Consultant"));
        }

        [Test]
        public async Task GetDoctorDetails_WhenDoctorDoesNotExist_ReturnsNull()
        {
            var doctorId = 1;

            _mockAdminRepository.Setup(repo => repo.GetDoctorByIdAsync(doctorId))
                .ReturnsAsync((Doctor)null);

            var result = await _adminService.GetDoctorDetails(doctorId);

            Assert.That(result, Is.Null);
        }

    }
}
