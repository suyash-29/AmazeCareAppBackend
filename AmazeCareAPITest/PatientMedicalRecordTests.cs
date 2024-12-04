using AmazeCareAPI.Dtos;
using AmazeCareAPI.Exceptions;
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
    public class PatientMedicalRecordTests
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
        public async Task GetMedicalHistory_ShouldReturnMedicalHistory_WhenPatientExists()
        {
            int userId = 1;
            var patient = new Patient { PatientID = 1, UserID = userId };
            var medicalHistory = new List<PatientMedicalRecordDto>
            {
                new PatientMedicalRecordDto
                {
                    MedicalRecordID = 1,
                    AppointmentDate = DateTime.Now.AddMonths(-1),
                    DoctorName = "Dr. John Smith",
                    Symptoms = "Fever",
                    PhysicalExamination = "Normal",
                    TreatmentPlan = "Rest and hydration",
                    FollowUpDate = DateTime.Now.AddMonths(1),
                    Tests = new List<TestDto>
                    {
                        new TestDto { TestID = 1, TestName = "Blood Test", TestPrice = 100 }
                    },
                    Prescriptions = new List<PrescriptionDto>
                    {
                        new PrescriptionDto { MedicationID = 1, MedicationName = "Paracetamol", Dosage = "500mg", DurationDays = 5, Quantity = 10, TotalPrice = 50 }
                    },
                    BillingDetails = new BillingDto
                    {
                        BillingID = 1,
                        ConsultationFee = 200,
                        TotalTestsPrice = 100,
                        TotalMedicationsPrice = 50,
                        GrandTotal = 350,
                        Status = "Paid"
                    }
                }
            };

            _mockPatientRepository.Setup(repo => repo.GetPatientByUserIdAsync(userId))
                .ReturnsAsync(patient);
            _mockPatientRepository.Setup(repo => repo.GetMedicalHistoryAsync(patient.PatientID))
                .ReturnsAsync(medicalHistory);

            var result = await _patientService.GetMedicalHistory(userId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].DoctorName, Is.EqualTo("Dr. John Smith"));
            Assert.That(result[0].Symptoms, Is.EqualTo("Fever"));
            Assert.That(result[0].BillingDetails.GrandTotal, Is.EqualTo(350));
        }

        [Test]
        public async Task GetMedicalHistory_ShouldThrowPatientNotFoundException_WhenPatientDoesNotExist()
        {
            int userId = 999;

            _mockPatientRepository.Setup(repo => repo.GetPatientByUserIdAsync(userId))
                .ReturnsAsync((Patient)null);

            var ex = Assert.ThrowsAsync<ServiceException>(async () => await _patientService.GetMedicalHistory(userId));

            Assert.That(ex.Message, Is.EqualTo("An error occurred while retrieving medical history."));
            var innerEx = ex.InnerException as PatientNotFoundException;
            Assert.That(innerEx, Is.Not.Null);
            Assert.That(innerEx.Message, Is.EqualTo("Patient not found."));
        }

        [Test]
        public async Task GetTestDetails_ShouldReturnTestDetails_WhenPatientExists()
        {
            int userId = 1;
            var patientId = 1;

            var testDetails = new List<PatientTestDetailDto>
            {
                new PatientTestDetailDto { AppointmentId = 1, DoctorName = "Dr. Smith", TestId = 101, TestName = "Blood Test", TestPrice = 100 },
                new PatientTestDetailDto { AppointmentId = 2, DoctorName = "Dr. Johnson", TestId = 102, TestName = "X-Ray", TestPrice = 200 }
            };

            _mockPatientRepository.Setup(repo => repo.GetPatientIdByUserIdAsync(userId)).ReturnsAsync(patientId);
            _mockPatientRepository.Setup(repo => repo.GetTestDetailsByPatientIdAsync(patientId)).ReturnsAsync(testDetails);

            var result = await _patientService.GetTestDetails(userId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].TestName, Is.EqualTo("Blood Test"));
            Assert.That(result[0].TestPrice, Is.EqualTo(100));
        }

        [Test]
        public async Task GetPrescriptionDetails_ShouldReturnPrescriptionDetails_WhenPatientExists()
        {
            int userId = 1;
            var patientId = 1;

            var prescriptionDetails = new List<PatientPrescriptionDetailDto>
            {
                new PatientPrescriptionDetailDto
                {
                    AppointmentId = 1,
                    DoctorName = "Dr. Smith",
                    MedicationName = "Paracetamol",
                    Dosage = "500mg",
                    DurationDays = 5,
                    Quantity = 10
                },
                new PatientPrescriptionDetailDto
                {
                    AppointmentId = 2,
                    DoctorName = "Dr. Johnson",
                    MedicationName = "Amoxicillin",
                    Dosage = "250mg",
                    DurationDays = 7,
                    Quantity = 14
                }
            };

            _mockPatientRepository.Setup(repo => repo.GetPatientIdByUserIdAsync(userId)).ReturnsAsync(patientId);
            _mockPatientRepository.Setup(repo => repo.GetPrescriptionDetailsByPatientIdAsync(patientId)).ReturnsAsync(prescriptionDetails);

            var result = await _patientService.GetPrescriptionDetails(userId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].MedicationName, Is.EqualTo("Paracetamol"));
            Assert.That(result[0].Dosage, Is.EqualTo("500mg"));
        }

        [Test]
        public async Task GetBillingDetails_ShouldReturnBillingDetails_WhenPatientExists()
        {
            int userId = 1;
            var patientId = 1;

            var billingDetails = new List<BillingDto>
            {
                new BillingDto
                {
                    BillingID = 1,
                    ConsultationFee = 100.00m,
                    TotalTestsPrice = 50.00m,
                    TotalMedicationsPrice = 25.00m,
                    GrandTotal = 175.00m,
                    Status = "Paid"
                },
                new BillingDto
                {
                    BillingID = 2,
                    ConsultationFee = 150.00m,
                    TotalTestsPrice = 75.00m,
                    TotalMedicationsPrice = 40.00m,
                    GrandTotal = 265.00m,
                    Status = "Pending"
                }
            };

            _mockPatientRepository.Setup(repo => repo.GetPatientIdByUserIdAsync(userId)).ReturnsAsync(patientId);
            _mockPatientRepository.Setup(repo => repo.GetBillingDetailsByPatientIdAsync(patientId)).ReturnsAsync(billingDetails);

            var result = await _patientService.GetBillingDetails(userId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].BillingID, Is.EqualTo(1));
            Assert.That(result[0].ConsultationFee, Is.EqualTo(100.00m));
            Assert.That(result[0].TotalTestsPrice, Is.EqualTo(50.00m));
            Assert.That(result[0].TotalMedicationsPrice, Is.EqualTo(25.00m));
            Assert.That(result[0].GrandTotal, Is.EqualTo(175.00m));
            Assert.That(result[0].Status, Is.EqualTo("Paid"));

            Assert.That(result[1].BillingID, Is.EqualTo(2));
            Assert.That(result[1].ConsultationFee, Is.EqualTo(150.00m));
            Assert.That(result[1].TotalTestsPrice, Is.EqualTo(75.00m));
            Assert.That(result[1].TotalMedicationsPrice, Is.EqualTo(40.00m));
            Assert.That(result[1].GrandTotal, Is.EqualTo(265.00m));
            Assert.That(result[1].Status, Is.EqualTo("Pending"));
        }
    }
}
