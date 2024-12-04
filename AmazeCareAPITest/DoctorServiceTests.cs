using AmazeCareAPI.Dtos;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories.Interface;
using AmazeCareAPI.Services;
using AmazeCareAPI.Services.Interface;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazeCareAPITest
{
    [TestFixture]
    internal class DoctorServiceTests
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
        public async Task ConductConsultation_ShouldReturnTrue_WhenAllDataIsValid()
        {
            int doctorId = 1;
            int appointmentId = 101;
            decimal consultationFee = 100.0m;

            var appointment = new Appointment
            {
                AppointmentID = appointmentId,
                DoctorID = doctorId,
                PatientID = 5,
                Status = "Scheduled"
            };

            var recordDto = new CreateMedicalRecordDto
            {
                Symptoms = "Fever, Cough",
                PhysicalExamination = "Normal",
                TreatmentPlan = "Rest and medication",
                FollowUpDate = DateTime.Now.AddDays(7),
                TestIDs = new List<int> { 1, 2 },
                Prescriptions = new List<CreatePrescriptionDto>
                {
                    new CreatePrescriptionDto { MedicationID = 1, Dosage = "Twice a day", DurationDays = 5, Quantity = 10 }
                }
            };

            var tests = new List<Test>
            {
                new Test { TestID = 1, TestPrice = 50.0m },
                new Test { TestID = 2, TestPrice = 30.0m }
            };

            var medication = new Medication { MedicationID = 1, PricePerUnit = 20.0m, MedicationName = "Paracetamol" };

            _mockDoctorRepository.Setup(repo => repo.GetScheduledAppointmentAsync(doctorId, appointmentId))
                .ReturnsAsync(appointment);

            _mockDoctorRepository.Setup(repo => repo.GetTestsByIdsAsync(recordDto.TestIDs))
                .ReturnsAsync(tests);

            _mockDoctorRepository.Setup(repo => repo.GetMedicationByIdAsync(1))
                .ReturnsAsync(medication);

            _mockDoctorRepository.Setup(repo => repo.AddMedicalRecordAsync(It.IsAny<MedicalRecord>()))
                .Returns(Task.CompletedTask);

            _mockDoctorRepository.Setup(repo => repo.AddMedicalRecordTestsAsync(It.IsAny<List<MedicalRecordTest>>()))
                .Returns(Task.CompletedTask);

            _mockDoctorRepository.Setup(repo => repo.AddPrescriptionsAsync(It.IsAny<List<Prescription>>()))
                .Returns(Task.CompletedTask);

            _mockDoctorRepository.Setup(repo => repo.UpdateMedicalRecordTotalPriceAsync(It.IsAny<MedicalRecord>()))
                .Returns(Task.CompletedTask);

            _mockDoctorRepository.Setup(repo => repo.AddBillingAsync(It.IsAny<Billing>()))
                .Returns(Task.CompletedTask);

            _mockDoctorRepository.Setup(repo => repo.UpdateBillingAsync(It.IsAny<Billing>()))
                .Returns(Task.CompletedTask);

            _mockDoctorRepository.Setup(repo => repo.UpdatePrescriptionBillingIdsAsync(It.IsAny<List<Prescription>>()))
                .Returns(Task.CompletedTask);

            _mockDoctorRepository.Setup(repo => repo.UpdateAppointmentAsync(It.IsAny<Appointment>()))
                .Returns(Task.CompletedTask);

            var result = await _doctorService.ConductConsultation(doctorId, appointmentId, recordDto, consultationFee);

            Assert.That(result, Is.True);
            _mockDoctorRepository.Verify(repo => repo.UpdateAppointmentAsync(It.Is<Appointment>(a => a.Status == "Completed")), Times.Once);
        }

        [Test]
        public async Task ConductConsultation_ShouldReturnFalse_WhenAppointmentDoesNotExist()
        {
            int doctorId = 1;
            int appointmentId = 101;
            var recordDto = new CreateMedicalRecordDto();
            decimal consultationFee = 100.0m;

            _mockDoctorRepository.Setup(repo => repo.GetScheduledAppointmentAsync(doctorId, appointmentId))
                .ReturnsAsync((Appointment)null);

            var result = await _doctorService.ConductConsultation(doctorId, appointmentId, recordDto, consultationFee);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ConductConsultation_ShouldReturnFalse_WhenAppointmentIsNotScheduled()
        {
            int doctorId = 1;
            int appointmentId = 101;
            var appointment = new Appointment { AppointmentID = appointmentId, DoctorID = doctorId, Status = "Completed" };
            var recordDto = new CreateMedicalRecordDto();
            decimal consultationFee = 100.0m;

            _mockDoctorRepository.Setup(repo => repo.GetScheduledAppointmentAsync(doctorId, appointmentId))
                .ReturnsAsync(appointment);

            var result = await _doctorService.ConductConsultation(doctorId, appointmentId, recordDto, consultationFee);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ConductConsultation_ShouldThrowApplicationException_WhenRepositoryThrowsException()
        {
            int doctorId = 1;
            int appointmentId = 101;
            var recordDto = new CreateMedicalRecordDto();
            decimal consultationFee = 100.0m;

            _mockDoctorRepository.Setup(repo => repo.GetScheduledAppointmentAsync(doctorId, appointmentId))
                .ThrowsAsync(new Exception("Database error"));

            var exception = Assert.ThrowsAsync<ApplicationException>(() =>
                _doctorService.ConductConsultation(doctorId, appointmentId, recordDto, consultationFee));
            Assert.That(exception.Message, Contains.Substring($"Error conducting consultation for appointment {appointmentId} with doctor {doctorId}."));
        }
    }
}
