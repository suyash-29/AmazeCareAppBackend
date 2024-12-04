using AmazeCareAPI.Dtos;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories.Interface;
using AmazeCareAPI.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazeCareAPITest
{
    [TestFixture]
    public class DoctorAppointmentsTests
    {
        private Mock<IDoctorRepository> _mockDoctorRepository;
        private DoctorService _doctorService;

        [SetUp]
        public void SetUp()
        {
            _mockDoctorRepository = new Mock<IDoctorRepository>();
            _doctorService = new DoctorService(_mockDoctorRepository.Object);
        }

        [Test]
        public async Task GetAppointmentsByStatus_ShouldReturnAppointments_WhenValidDoctorIdAndStatus()
        {
            int doctorId = 1;
            string status = "Scheduled";
            var appointments = new List<AppointmentWithPatientDto>
            {
                new AppointmentWithPatientDto { AppointmentID = 101, PatientName = "John Doe", AppointmentDate = DateTime.Now },
                new AppointmentWithPatientDto { AppointmentID = 102, PatientName = "Jane Smith", AppointmentDate = DateTime.Now.AddDays(1) }
            };

            _mockDoctorRepository.Setup(repo => repo.GetAppointmentsByStatusAsync(doctorId, status))
                .ReturnsAsync(appointments);

            var result = await _doctorService.GetAppointmentsByStatus(doctorId, status);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].PatientName, Is.EqualTo("John Doe"));
        }

        [Test]
        public async Task GetAppointmentsByStatus_ShouldThrowApplicationException_WhenRepositoryThrowsException()
        {
            int doctorId = 1;
            string status = "Scheduled";

            _mockDoctorRepository.Setup(repo => repo.GetAppointmentsByStatusAsync(doctorId, status))
                .ThrowsAsync(new Exception("Database error"));

            var exception = Assert.ThrowsAsync<ApplicationException>(() =>
                _doctorService.GetAppointmentsByStatus(doctorId, status));
            Assert.That(exception.Message, Contains.Substring($"Error fetching appointments with status '{status}' for doctor {doctorId}."));
        }

        [Test]
        public async Task ApproveAppointmentRequest_ShouldReturnTrue_WhenAppointmentExists()
        {
            int doctorId = 1;
            int appointmentId = 1;

            var appointment = new Appointment
            {
                AppointmentID = appointmentId,
                DoctorID = doctorId,
                Status = "Requested"
            };

            _mockDoctorRepository.Setup(repo => repo.GetRequestedAppointmentAsync(doctorId, appointmentId))
                .ReturnsAsync(appointment);

            _mockDoctorRepository.Setup(repo => repo.UpdateAppointmentAsync(It.IsAny<Appointment>()))
                .Returns(Task.CompletedTask);

            var result = await _doctorService.ApproveAppointmentRequest(doctorId, appointmentId);

            Assert.That(result, Is.True);
            Assert.That(appointment.Status, Is.EqualTo("Scheduled"));
            _mockDoctorRepository.Verify(repo => repo.UpdateAppointmentAsync(It.Is<Appointment>(a => a.Status == "Scheduled")), Times.Once);
        }

        [Test]
        public async Task ApproveAppointmentRequest_ShouldReturnFalse_WhenAppointmentDoesNotExist()
        {
            int doctorId = 1;
            int appointmentId = 1;

            _mockDoctorRepository.Setup(repo => repo.GetRequestedAppointmentAsync(doctorId, appointmentId))
                .ReturnsAsync((Appointment)null);

            var result = await _doctorService.ApproveAppointmentRequest(doctorId, appointmentId);

            Assert.That(result, Is.False);
            _mockDoctorRepository.Verify(repo => repo.UpdateAppointmentAsync(It.IsAny<Appointment>()), Times.Never);
        }

        [Test]
        public async Task CancelScheduledAppointment_ShouldReturnTrue_WhenAppointmentExists()
        {
            int doctorId = 1;
            int appointmentId = 1;

            var appointment = new Appointment
            {
                AppointmentID = appointmentId,
                DoctorID = doctorId,
                Status = "Scheduled"
            };

            _mockDoctorRepository.Setup(repo => repo.GetScheduledAppointmentAsync(doctorId, appointmentId))
                .ReturnsAsync(appointment);

            _mockDoctorRepository.Setup(repo => repo.UpdateAppointmentAsync(It.IsAny<Appointment>()))
                .Returns(Task.CompletedTask);

            var result = await _doctorService.CancelScheduledAppointment(doctorId, appointmentId);

            Assert.That(result, Is.True);
            Assert.That(appointment.Status, Is.EqualTo("Canceled"));
            _mockDoctorRepository.Verify(repo => repo.UpdateAppointmentAsync(It.Is<Appointment>(a => a.Status == "Canceled")), Times.Once);
        }

        [Test]
        public async Task CancelScheduledAppointment_ShouldReturnFalse_WhenAppointmentDoesNotExist()
        {
            int doctorId = 1;
            int appointmentId = 1;

            _mockDoctorRepository.Setup(repo => repo.GetScheduledAppointmentAsync(doctorId, appointmentId))
                .ReturnsAsync((Appointment)null);

            var result = await _doctorService.CancelScheduledAppointment(doctorId, appointmentId);

            Assert.That(result, Is.False);
            _mockDoctorRepository.Verify(repo => repo.UpdateAppointmentAsync(It.IsAny<Appointment>()), Times.Never);
        }

        [Test]
        public async Task RescheduleAppointmentAsync_ShouldReturnSuccess_WhenAppointmentIsValidAndNoConflict()
        {
            int doctorId = 1;
            int appointmentId = 1;
            var rescheduleDto = new AppointmentRescheduleDto
            {
                NewAppointmentDate = DateTime.Now.AddDays(1)
            };

            var appointment = new Appointment
            {
                AppointmentID = appointmentId,
                DoctorID = doctorId,
                AppointmentDate = DateTime.Now
            };

            _mockDoctorRepository.Setup(repo => repo.GetAppointmentByIdAndDoctorIdAsync(appointmentId, doctorId))
                .ReturnsAsync(appointment);

            _mockDoctorRepository.Setup(repo => repo.IsOnScheduleAsync(doctorId, rescheduleDto.NewAppointmentDate))
                .ReturnsAsync(true);

            _mockDoctorRepository.Setup(repo => repo.UpdateAppointmentAsync(It.IsAny<Appointment>()))
                .Returns(Task.CompletedTask);

            var (success, message) = await _doctorService.RescheduleAppointmentAsync(doctorId, appointmentId, rescheduleDto);

            Assert.That(success, Is.True);
            Assert.That(message, Is.EqualTo("Appointment rescheduled successfully."));
            _mockDoctorRepository.Verify(repo => repo.UpdateAppointmentAsync(It.Is<Appointment>(a => a.AppointmentDate == rescheduleDto.NewAppointmentDate)), Times.Once);
        }

        [Test]
        public async Task RescheduleAppointmentAsync_ShouldReturnFailure_WhenAppointmentDoesNotExist()
        {
            int doctorId = 1;
            int appointmentId = 1;
            var rescheduleDto = new AppointmentRescheduleDto
            {
                NewAppointmentDate = DateTime.Now.AddDays(1)
            };

            _mockDoctorRepository.Setup(repo => repo.GetAppointmentByIdAndDoctorIdAsync(appointmentId, doctorId))
                .ReturnsAsync((Appointment)null);

            var (success, message) = await _doctorService.RescheduleAppointmentAsync(doctorId, appointmentId, rescheduleDto);

            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("Appointment not found or unauthorized access."));
            _mockDoctorRepository.Verify(repo => repo.IsOnScheduleAsync(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Test]
        public async Task RescheduleAppointmentAsync_ShouldReturnFailure_WhenNewDateIsInPast()
        {
            int doctorId = 1;
            int appointmentId = 1;
            var rescheduleDto = new AppointmentRescheduleDto
            {
                NewAppointmentDate = DateTime.Now.AddDays(-1)
            };

            var appointment = new Appointment
            {
                AppointmentID = appointmentId,
                DoctorID = doctorId,
                AppointmentDate = DateTime.Now
            };

            _mockDoctorRepository.Setup(repo => repo.GetAppointmentByIdAndDoctorIdAsync(appointmentId, doctorId))
                .ReturnsAsync(appointment);

            var (success, message) = await _doctorService.RescheduleAppointmentAsync(doctorId, appointmentId, rescheduleDto);

            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("The new appointment date and time must be in the future."));
            _mockDoctorRepository.Verify(repo => repo.IsOnScheduleAsync(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Test]
        public async Task RescheduleAppointmentAsync_ShouldReturnFailure_WhenNewDateConflictsWithSchedule()
        {
            int doctorId = 1;
            int appointmentId = 1;
            var rescheduleDto = new AppointmentRescheduleDto
            {
                NewAppointmentDate = DateTime.Now.AddDays(1)
            };

            var appointment = new Appointment
            {
                AppointmentID = appointmentId,
                DoctorID = doctorId,
                AppointmentDate = DateTime.Now
            };

            _mockDoctorRepository.Setup(repo => repo.GetAppointmentByIdAndDoctorIdAsync(appointmentId, doctorId))
                .ReturnsAsync(appointment);

            _mockDoctorRepository.Setup(repo => repo.IsOnScheduleAsync(doctorId, rescheduleDto.NewAppointmentDate))
                .ReturnsAsync(false);

            var (success, message) = await _doctorService.RescheduleAppointmentAsync(doctorId, appointmentId, rescheduleDto);

            Assert.That(success, Is.False);
            Assert.That(message, Is.EqualTo("The new appointment date conflicts with the doctor's schedule."));
            _mockDoctorRepository.Verify(repo => repo.IsOnScheduleAsync(doctorId, rescheduleDto.NewAppointmentDate), Times.Once);
        }
    }
}
