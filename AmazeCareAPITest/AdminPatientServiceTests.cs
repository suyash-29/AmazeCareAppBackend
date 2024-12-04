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
    public class AdminPatientServiceTests
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
        public async Task GetPatientDetails_WhenPatientExists_ReturnsPatientDetails()
        {
            var patientId = 1;

            var patient = new Patient
            {
                PatientID = patientId,
                FullName = "John Doe",
                DateOfBirth = new DateTime(1985, 5, 15),
                Gender = "Male",
                ContactNumber = "1234567890",
                Email = "johndoe@example.com",
                Address = "123 Street, City",
                MedicalHistory = "No significant medical history"
            };

            _mockAdminRepository.Setup(repo => repo.GetPatientByIdAsync(patientId))
                .ReturnsAsync(patient);

            var result = await _adminService.GetPatientDetails(patientId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.PatientID, Is.EqualTo(patientId));
            Assert.That(result.FullName, Is.EqualTo("John Doe"));
            Assert.That(result.DateOfBirth, Is.EqualTo(new DateTime(1985, 5, 15)));
            Assert.That(result.Gender, Is.EqualTo("Male"));
            Assert.That(result.ContactNumber, Is.EqualTo("1234567890"));
            Assert.That(result.Email, Is.EqualTo("johndoe@example.com"));
            Assert.That(result.Address, Is.EqualTo("123 Street, City"));
            Assert.That(result.MedicalHistory, Is.EqualTo("No significant medical history"));
        }

        [Test]
        public async Task UpdatePatient_WhenPatientExists_UpdatesPatientDetails()
        {
            var patientDto = new PatientDto
            {
                PatientID = 1,
                FullName = "John Updated",
                Email = "johnupdated@example.com",
                DateOfBirth = new DateTime(1985, 5, 15),
                ContactNumber = "0987654321",
                Address = "Updated Address",
                MedicalHistory = "Updated medical history"
            };

            var patient = new Patient
            {
                PatientID = 1,
                FullName = "John Doe",
                DateOfBirth = new DateTime(1985, 5, 15),
                ContactNumber = "1234567890",
                Address = "Old Address",
                MedicalHistory = "Old medical history"
            };

            _mockAdminRepository.Setup(repo => repo.GetPatientByIdAsync(patientDto.PatientID))
                .ReturnsAsync(patient);
            _mockAdminRepository.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask);

            var result = await _adminService.UpdatePatient(patientDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.FullName, Is.EqualTo(patientDto.FullName));
            Assert.That(result.Email, Is.EqualTo(patientDto.Email));
            Assert.That(result.ContactNumber, Is.EqualTo(patientDto.ContactNumber));
            Assert.That(result.Address, Is.EqualTo(patientDto.Address));
            Assert.That(result.MedicalHistory, Is.EqualTo(patientDto.MedicalHistory));
        }

        [Test]
        public async Task DeletePatient_WhenPatientExists_DeletesPatientAndCancelsAppointments()
        {
            var userId = 1;
            var patientId = 1;

            var patient = new Patient
            {
                PatientID = patientId,
                UserID = userId,
                FullName = "John Doe",
                DateOfBirth = new DateTime(1985, 5, 15),
                ContactNumber = "1234567890",
                Address = "Old Address"
            };

            var appointments = new List<Appointment>
            {
                new Appointment { Status = "Scheduled" },
                new Appointment { Status = "Scheduled" }
            };

            _mockAdminRepository.Setup(repo => repo.GetPatientByIdAndUserIdAsync(patientId, userId))
                .ReturnsAsync(patient);
            _mockAdminRepository.Setup(repo => repo.GetScheduledAppointmentsByPatientIdAsync(patientId))
                .ReturnsAsync(appointments);
            _mockAdminRepository.Setup(repo => repo.DeleteUserAsync(userId)).Returns(Task.CompletedTask);
            _mockAdminRepository.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask);

            var result = await _adminService.DeletePatient(userId, patientId);

            Assert.That(result, Is.True);
            Assert.That(patient.UserID, Is.Null);
            Assert.That(appointments[0].Status, Is.EqualTo("Canceled"));
            Assert.That(appointments[1].Status, Is.EqualTo("Canceled"));
        }
    }
}
