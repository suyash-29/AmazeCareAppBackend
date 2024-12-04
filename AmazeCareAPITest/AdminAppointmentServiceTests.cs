using AmazeCareAPI.Dtos;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories.Interface;
using AmazeCareAPI.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace AmazeCareAPITest
{
    [TestFixture]
    public class AdminAppointmentServiceTests
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
        public async Task ViewAppointmentDetails_WhenAppointmentExists_ReturnsAppointment()
        {
            var appointmentId = 1;
            var appointment = new Appointment
            {
                AppointmentID = appointmentId,
                PatientID = 1,
                DoctorID = 1,
                AppointmentDate = DateTime.Now.AddDays(1),
                Status = "Scheduled",
                Symptoms = "Cough"
            };

            _mockAdminRepository.Setup(repo => repo.GetAppointmentByIdAsync(appointmentId))
                .ReturnsAsync(appointment);

 
            var result = await _adminService.ViewAppointmentDetails(appointmentId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.AppointmentID, Is.EqualTo(appointmentId));
            Assert.That(result.Status, Is.EqualTo("Scheduled"));
        }

        [Test]
        public async Task RescheduleAppointment_WhenAppointmentExistsAndDoctorIsOnSchedule_ReschedulesAppointment()
        {
            var appointmentId = 1;
            var rescheduleDto = new AppointmentRescheduleDto
            {
                NewAppointmentDate = DateTime.Now.AddDays(1) 
            };

            var appointment = new Appointment
            {
                AppointmentID = appointmentId,
                PatientID = 1,
                DoctorID = 1,
                AppointmentDate = DateTime.Now,
                Status = "Scheduled"
            };

            _mockAdminRepository.Setup(repo => repo.GetAppointmentWithDoctorByIdAsync(appointmentId))
                .ReturnsAsync(appointment);

            _mockAdminRepository.Setup(repo => repo.IsDoctorOnScheduleAsync(appointment.DoctorID, rescheduleDto.NewAppointmentDate))
                .ReturnsAsync(true);

            _mockAdminRepository.Setup(repo => repo.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _adminService.RescheduleAppointment(appointmentId, rescheduleDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.AppointmentDate, Is.EqualTo(rescheduleDto.NewAppointmentDate));
        }
    }
}
