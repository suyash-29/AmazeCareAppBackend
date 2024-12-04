using AmazeCareAPI.Dtos;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories.Interface;
using AmazeCareAPI.Services;
using Moq;

namespace AmazeCareAPITest
{
    [TestFixture]
    public class PatientAppointmentServiceTests
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
        public async Task GetPersonalInfoAsync_ShouldReturnPersonalInfo_WhenUserExists()
        {
            int userId = 1;

            var user = new User
            {
                UserID = userId,
                Username = "testuser",
                PasswordHash = "hashedpassword"
            };

            var patient = new Patient
            {
                UserID = userId,
                FullName = "John Doe",
                Email = "johndoe@example.com",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male",
                ContactNumber = "1234567890",
                Address = "123 Street",
                MedicalHistory = "None"
            };

            _mockPatientRepository.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            _mockPatientRepository.Setup(repo => repo.GetPatientByUserIdAsync(userId))
                .ReturnsAsync(patient);

            var result = await _patientService.GetPersonalInfoAsync(userId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.Username, Is.EqualTo("testuser"));
            Assert.That(result.FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public async Task ScheduleAppointment_ShouldScheduleAppointment_WhenDoctorIsAvailable()
        {
            int userId = 1;

            var patient = new Patient
            {
                PatientID = 1,
                UserID = userId
            };

            var bookingDto = new AppointmentBookingDto
            {
                DoctorID = 2,
                AppointmentDate = DateTime.Now.AddDays(1),
                Symptoms = "Cough and fever"
            };

            _mockPatientRepository.Setup(repo => repo.GetPatientByUserIdAsync(userId))
                .ReturnsAsync(patient);

            _mockPatientRepository.Setup(repo => repo.IsDoctorOnSchedule(bookingDto.DoctorID, bookingDto.AppointmentDate))
                .ReturnsAsync(true);

            var (appointment, message) = await _patientService.ScheduleAppointment(userId, bookingDto);

            Assert.That(appointment, Is.Not.Null);
            Assert.That(appointment.Status, Is.EqualTo("Requested"));
            Assert.That(message, Is.EqualTo("Appointment requested successfully."));
            _mockPatientRepository.Verify(repo => repo.AddAppointmentAsync(It.IsAny<Appointment>()), Times.Once);
        }

        [Test]
        public async Task ScheduleAppointment_ShouldThrowServiceException_WhenDoctorNotAvailable()
        {
            int userId = 22;

            var patient = new Patient
            {
                PatientID = 17,
                UserID = userId
            };

            var bookingDto = new AppointmentBookingDto
            {
                DoctorID = 15,
                AppointmentDate = DateTime.Now.AddDays(2),
                Symptoms = "Cough and fever"
            };

            _mockPatientRepository.Setup(repo => repo.GetPatientByUserIdAsync(userId))
                .ReturnsAsync(patient);

            _mockPatientRepository.Setup(repo => repo.IsDoctorOnSchedule(bookingDto.DoctorID, bookingDto.AppointmentDate))
                .ReturnsAsync(false);

            var ex = Assert.ThrowsAsync<ServiceException>(() => _patientService.ScheduleAppointment(userId, bookingDto));
            Assert.That(ex.InnerException, Is.InstanceOf<InvalidOperationException>());
            Assert.That(ex.InnerException.Message, Is.EqualTo("The selected appointment date and time doctor is not available."));
        }

        [Test]
        public async Task GetAppointments_ShouldReturnAppointments_WhenPatientExists()
        {
            int userId = 1;
            var patient = new Patient { PatientID = 1, UserID = userId };
            var appointments = new List<AppointmentWithDoctorDto>
            {
                new AppointmentWithDoctorDto { AppointmentID = 1, DoctorName = "Dr. John Doe", AppointmentDate = DateTime.Now }
            };

            _mockPatientRepository.Setup(repo => repo.GetPatientByUserIdAsync(userId))
                .ReturnsAsync(patient);
            _mockPatientRepository.Setup(repo => repo.GetAppointmentsByPatientIdAsync(patient.PatientID))
                .ReturnsAsync(appointments);

            var result = await _patientService.GetAppointments(userId);

            var resultList = result.ToList();
            Assert.That(resultList, Is.Not.Null);
            Assert.That(resultList.Count, Is.EqualTo(1));
            Assert.That(resultList.First().DoctorName, Is.EqualTo("Dr. John Doe"));
        }

        [Test]
        public async Task GetAppointments_ShouldThrowPatientNotFoundException_WhenPatientDoesNotExist()
        {
            int userId = 99;
            _mockPatientRepository.Setup(repo => repo.GetPatientByUserIdAsync(userId))
                .ReturnsAsync((Patient)null);

            var ex = Assert.ThrowsAsync<ServiceException>(async () => await _patientService.GetAppointments(userId));
            Assert.That(ex.Message, Is.EqualTo("An error occurred while retrieving appointments."));
            Assert.That(ex.InnerException, Is.InstanceOf<PatientNotFoundException>());
            Assert.That(ex.InnerException.Message, Is.EqualTo("Patient not found."));
        }

        [Test]
        public async Task CancelAppointment_ShouldCancel_WhenValidAppointment()
        {
            int userId = 1;
            int appointmentId = 1;
            var patient = new Patient { PatientID = 1, UserID = userId };
            var appointment = new Appointment { AppointmentID = appointmentId, PatientID = patient.PatientID, Status = "Scheduled" };

            _mockPatientRepository.Setup(repo => repo.GetPatientByUserIdAsync(userId))
                .ReturnsAsync(patient);
            _mockPatientRepository.Setup(repo => repo.GetAppointmentByIdAsync(patient.PatientID, appointmentId))
                .ReturnsAsync(appointment);

            var result = await _patientService.CancelAppointment(userId, appointmentId);

            Assert.That(result, Is.True);
            Assert.That(appointment.Status, Is.EqualTo("Canceled"));
            _mockPatientRepository.Verify(repo => repo.UpdateAppointmentAsync(It.IsAny<Appointment>()), Times.Once);
            _mockPatientRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task CancelAppointment_ShouldThrowInvalidOperationException_WhenAppointmentAlreadyCanceled()
        {
            int userId = 1;
            int appointmentId = 1;
            var patient = new Patient { PatientID = 1, UserID = userId };
            var appointment = new Appointment { AppointmentID = appointmentId, PatientID = patient.PatientID, Status = "Canceled" };

            _mockPatientRepository.Setup(repo => repo.GetPatientByUserIdAsync(userId))
                .ReturnsAsync(patient);
            _mockPatientRepository.Setup(repo => repo.GetAppointmentByIdAsync(patient.PatientID, appointmentId))
                .ReturnsAsync(appointment);

            var ex = Assert.ThrowsAsync<ServiceException>(async () => await _patientService.CancelAppointment(userId, appointmentId));

            Assert.That(ex.Message, Is.EqualTo("An error occurred while canceling the appointment."));
            var innerEx = ex.InnerException as InvalidOperationException;
            Assert.That(innerEx, Is.Not.Null);
            Assert.That(innerEx.Message, Is.EqualTo("Appointment is already canceled."));
        }

        [Test]
        public async Task RescheduleAppointment_ShouldRescheduleAppointment_WhenValidInputs()
        {
            int userId = 1;
            int appointmentId = 1;
            var rescheduleDto = new AppointmentRescheduleDto { NewAppointmentDate = DateTime.Now.AddDays(2) };

            var patientId = 1;
            var patient = new Patient { PatientID = patientId, UserID = userId };
            var appointment = new Appointment { AppointmentID = appointmentId, PatientID = patientId, Status = "Scheduled", DoctorID = 1 };

            _mockPatientRepository.Setup(repo => repo.GetPatientIdByUserIdAsync(userId)).ReturnsAsync(patientId);
            _mockPatientRepository.Setup(repo => repo.GetAppointmentByIdAndPatientIdAsync(appointmentId, patientId)).ReturnsAsync(appointment);
            _mockPatientRepository.Setup(repo => repo.IsDoctorOnScheduleAsync(appointment.DoctorID, rescheduleDto.NewAppointmentDate)).ReturnsAsync(true);

            var (rescheduledAppointment, message) = await _patientService.RescheduleAppointment(userId, appointmentId, rescheduleDto);

            Assert.That(rescheduledAppointment, Is.Not.Null);
            Assert.That(rescheduledAppointment.Status, Is.EqualTo("Requested"));
            Assert.That(rescheduledAppointment.AppointmentDate, Is.EqualTo(rescheduleDto.NewAppointmentDate));
            Assert.That(message, Is.EqualTo("Appointment rescheduled successfully and status updated to 'Requested'."));
            _mockPatientRepository.Verify(repo => repo.UpdateAppointmentAsync(It.IsAny<Appointment>()), Times.Once);
            _mockPatientRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task RescheduleAppointment_ShouldThrowServiceException_WhenDoctorNotAvailable()
        {
            int userId = 1;
            int appointmentId = 1;
            var rescheduleDto = new AppointmentRescheduleDto { NewAppointmentDate = DateTime.Now.AddDays(2) };

            var patientId = 1;
            var patient = new Patient { PatientID = patientId, UserID = userId };
            var appointment = new Appointment { AppointmentID = appointmentId, PatientID = patientId, Status = "Scheduled", DoctorID = 1 };

            _mockPatientRepository.Setup(repo => repo.GetPatientIdByUserIdAsync(userId)).ReturnsAsync(patientId);
            _mockPatientRepository.Setup(repo => repo.GetAppointmentByIdAndPatientIdAsync(appointmentId, patientId)).ReturnsAsync(appointment);
            _mockPatientRepository.Setup(repo => repo.IsDoctorOnScheduleAsync(appointment.DoctorID, rescheduleDto.NewAppointmentDate)).ReturnsAsync(false);

            var ex = Assert.ThrowsAsync<ServiceException>(() => _patientService.RescheduleAppointment(userId, appointmentId, rescheduleDto));
            Assert.That(ex.Message, Is.EqualTo("An error occurred while rescheduling the appointment."));
        }
    }
}
