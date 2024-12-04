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
    public class DoctorMedicalRecordServiceTests
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
        public async Task UpdateMedicalRecord_ShouldUpdateRecord_WhenRecordExists()
        {
            int doctorId = 1;
            int recordId = 1;
            int patientId = 1;
            var updateDto = new UpdateMedicalRecordDto
            {
                Symptoms = "Updated Symptoms",
                PhysicalExamination = "Updated Examination",
                TreatmentPlan = "Updated Plan",
                FollowUpDate = DateTime.Now.AddDays(7)
            };

            var existingRecord = new MedicalRecord
            {
                RecordID = recordId,
                DoctorID = doctorId,
                PatientID = patientId,
                Symptoms = "Old Symptoms",
                PhysicalExamination = "Old Examination",
                TreatmentPlan = "Old Plan",
                FollowUpDate = DateTime.Now
            };

            _mockDoctorRepository.Setup(repo => repo.GetMedicalRecordAsync(doctorId, recordId, patientId))
                .ReturnsAsync(existingRecord);

            _mockDoctorRepository.Setup(repo => repo.UpdateMedicalRecordAsync(It.IsAny<MedicalRecord>()))
                .Returns(Task.CompletedTask);

            var result = await _doctorService.UpdateMedicalRecord(doctorId, recordId, patientId, updateDto);

            Assert.That(result, Is.True);
            Assert.That(existingRecord.Symptoms, Is.EqualTo("Updated Symptoms"));
            Assert.That(existingRecord.PhysicalExamination, Is.EqualTo("Updated Examination"));
            Assert.That(existingRecord.TreatmentPlan, Is.EqualTo("Updated Plan"));
            Assert.That(existingRecord.FollowUpDate, Is.EqualTo(updateDto.FollowUpDate));
            _mockDoctorRepository.Verify(repo => repo.UpdateMedicalRecordAsync(It.IsAny<MedicalRecord>()), Times.Once);
        }

        [Test]
        public async Task UpdateMedicalRecord_ShouldReturnFalse_WhenRecordDoesNotExist()
        {
            int doctorId = 1;
            int recordId = 1;
            int patientId = 1;
            var updateDto = new UpdateMedicalRecordDto();

            _mockDoctorRepository.Setup(repo => repo.GetMedicalRecordAsync(doctorId, recordId, patientId))
                .ReturnsAsync((MedicalRecord)null);

            var result = await _doctorService.UpdateMedicalRecord(doctorId, recordId, patientId, updateDto);

            Assert.That(result, Is.False);
            _mockDoctorRepository.Verify(repo => repo.UpdateMedicalRecordAsync(It.IsAny<MedicalRecord>()), Times.Never);
        }

        [Test]
        public async Task GetMedicalRecordsByPatientIdAsync_ShouldThrowException_WhenNoRecordsExist()
        {
            int patientId = 1;

            _mockDoctorRepository.Setup(repo => repo.GetAppointmentsWithMedicalRecordsAndDetailsAsync(patientId))
                .ReturnsAsync(new List<Appointment>());

            var exception = Assert.ThrowsAsync<Exception>(async () =>
                await _doctorService.GetMedicalRecordsByPatientIdAsync(patientId));

            StringAssert.Contains("No medical records found for the given patient ID.", exception.Message);
            _mockDoctorRepository.Verify(repo => repo.GetAppointmentsWithMedicalRecordsAndDetailsAsync(patientId), Times.Once);
        }

        [Test]
        public async Task GetBillsByDoctorIdAsync_ShouldReturnBillingDetails_WhenBillsExist()
        {
            int doctorId = 1;
            var bills = new List<Billing>
            {
                new Billing
                {
                    BillingID = 1,
                    DoctorID = doctorId,
                    Doctor = new Doctor { FullName = "Dr. John Doe" },
                    PatientID = 2,
                    Patient = new Patient { FullName = "Jane Doe" },
                    ConsultationFee = 100,
                    TotalTestsPrice = 200,
                    TotalMedicationsPrice = 300,
                    GrandTotal = 600,
                    Status = "Unpaid"
                }
            };

            _mockDoctorRepository.Setup(repo => repo.GetBillsByDoctorIdAsync(doctorId))
                .ReturnsAsync(bills);

            var result = await _doctorService.GetBillsByDoctorIdAsync(doctorId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            var bill = result.First();
            Assert.That(bill.BillingID, Is.EqualTo(1));
            Assert.That(bill.DoctorName, Is.EqualTo("Dr. John Doe"));
            Assert.That(bill.PatientName, Is.EqualTo("Jane Doe"));
            _mockDoctorRepository.Verify(repo => repo.GetBillsByDoctorIdAsync(doctorId), Times.Once);
        }

        [Test]
        public async Task GetBillsByDoctorIdAsync_ShouldThrowException_WhenNoBillsExist()
        {
            int doctorId = 1;
            _mockDoctorRepository.Setup(repo => repo.GetBillsByDoctorIdAsync(doctorId))
                .ReturnsAsync(new List<Billing>());

            var exception = Assert.ThrowsAsync<Exception>(async () => await _doctorService.GetBillsByDoctorIdAsync(doctorId));

            StringAssert.StartsWith("An error occurred while fetching bills:", exception.Message);
            Assert.That(exception.Message, Contains.Substring("No bills found for the doctor."));
            _mockDoctorRepository.Verify(repo => repo.GetBillsByDoctorIdAsync(doctorId), Times.Once);
        }

        [Test]
        public async Task UpdateBillingStatusAsync_ShouldMarkAsPaid_WhenBillingRecordExistsAndIsUnpaid()
        {
            int billingId = 1;
            int doctorId = 1;

            var billing = new Billing
            {
                BillingID = billingId,
                DoctorID = doctorId,
                Status = "Unpaid"
            };

            _mockDoctorRepository.Setup(repo => repo.GetBillingByIdAndDoctorIdAsync(billingId, doctorId))
                .ReturnsAsync(billing);

            _mockDoctorRepository.Setup(repo => repo.UpdateBillingAsync(It.IsAny<Billing>()))
                .Returns(Task.CompletedTask);

            var result = await _doctorService.UpdateBillingStatusAsync(billingId, doctorId);

            Assert.That(result, Is.True);
            Assert.That(billing.Status, Is.EqualTo("Paid"));
            _mockDoctorRepository.Verify(repo => repo.UpdateBillingAsync(It.Is<Billing>(b => b.Status == "Paid")), Times.Once);
        }

        [Test]
        public async Task UpdateBillingStatusAsync_ShouldThrowException_WhenBillingRecordAlreadyPaid()
        {
            int billingId = 1;
            int doctorId = 1;

            var billing = new Billing
            {
                BillingID = billingId,
                DoctorID = doctorId,
                Status = "Paid"
            };

            _mockDoctorRepository.Setup(repo => repo.GetBillingByIdAndDoctorIdAsync(billingId, doctorId))
                .ReturnsAsync(billing);

            var exception = Assert.ThrowsAsync<Exception>(async () => await _doctorService.UpdateBillingStatusAsync(billingId, doctorId));

            StringAssert.StartsWith("An error occurred while updating billing status:", exception.Message);
            Assert.That(exception.Message, Contains.Substring("Billing record is already marked as paid."));
            _mockDoctorRepository.Verify(repo => repo.UpdateBillingAsync(It.IsAny<Billing>()), Times.Never);
        }

        [Test]
        public async Task GetTestsAsync_ShouldReturnTests_WhenTestsExist()
        {
            var tests = new List<TestDto>
            {
                new TestDto { TestID = 1, TestName = "Blood Test", TestPrice = 200 }
            };

            _mockDoctorRepository.Setup(repo => repo.GetAllTestsAsync())
                .ReturnsAsync(tests);

            var result = await _doctorService.GetTestsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().TestName, Is.EqualTo("Blood Test"));
            _mockDoctorRepository.Verify(repo => repo.GetAllTestsAsync(), Times.Once);
        }

        [Test]
        public async Task GetTestsAsync_ShouldThrowException_WhenNoTestsExist()
        {
            _mockDoctorRepository.Setup(repo => repo.GetAllTestsAsync())
                .ReturnsAsync(new List<TestDto>());

            var exception = Assert.ThrowsAsync<Exception>(async () => await _doctorService.GetTestsAsync());

            StringAssert.StartsWith("An error occurred while fetching tests:", exception.Message);
            Assert.That(exception.Message, Contains.Substring("No tests available."));
            _mockDoctorRepository.Verify(repo => repo.GetAllTestsAsync(), Times.Once);
        }

        [Test]
        public async Task GetMedicationsAsync_ShouldReturnMedications_WhenMedicationsExist()
        {
            var medications = new List<MedicationDto>
            {
                new MedicationDto { MedicationID = 1, MedicationName = "Paracetamol", PricePerUnit = 50 }
            };

            _mockDoctorRepository.Setup(repo => repo.GetAllMedicationsAsync())
                .ReturnsAsync(medications);

            var result = await _doctorService.GetMedicationsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().MedicationName, Is.EqualTo("Paracetamol"));
            _mockDoctorRepository.Verify(repo => repo.GetAllMedicationsAsync(), Times.Once);
        }

        [Test]
        public async Task GetMedicationsAsync_ShouldThrowException_WhenNoMedicationsExist()
        {
            _mockDoctorRepository.Setup(repo => repo.GetAllMedicationsAsync())
                .ReturnsAsync(new List<MedicationDto>());

            var exception = Assert.ThrowsAsync<Exception>(async () => await _doctorService.GetMedicationsAsync());

            StringAssert.StartsWith("An error occurred while fetching medications:", exception.Message);
            Assert.That(exception.Message, Contains.Substring("No medications available."));
            _mockDoctorRepository.Verify(repo => repo.GetAllMedicationsAsync(), Times.Once);
        }
    }
}
