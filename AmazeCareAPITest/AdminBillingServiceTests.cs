using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories.Interface;
using AmazeCareAPI.Services;
using Moq;
using AmazeCareAPI.Dtos;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazeCareAPITest
{
    [TestFixture]
    public class AdminBillingServiceTests
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
        public async Task GetBillingDetailsWithNamesAsync_WhenBillingDetailsExist_ReturnsBillingDetails()
        {
            var billingDetails = new List<Billing>
            {
                new Billing
                {
                    BillingID = 1,
                    PatientID = 101,
                    Patient = new Patient { FullName = "John Doe" },
                    DoctorID = 201,
                    Doctor = new Doctor { FullName = "Dr. Smith" },
                    ConsultationFee = 150.0m,
                    TotalTestsPrice = 200.0m,
                    TotalMedicationsPrice = 50.0m,
                    GrandTotal = 400.0m,
                    Status = "Paid"
                }
            };

            _mockAdminRepository
                .Setup(repo => repo.GetBillingDetailsWithNamesAsync())
                .ReturnsAsync(billingDetails);

            var result = await _adminService.GetBillingDetailsWithNamesAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().PatientName, Is.EqualTo("John Doe"));
            Assert.That(result.First().DoctorName, Is.EqualTo("Dr. Smith"));
            Assert.That(result.First().GrandTotal, Is.EqualTo(400.0m));
            _mockAdminRepository.Verify(repo => repo.GetBillingDetailsWithNamesAsync(), Times.Once);
        }

        [Test]
        public async Task GetBillingDetailsWithNamesAsync_WhenExceptionOccurs_ThrowsApplicationException()
        {
            _mockAdminRepository
                .Setup(repo => repo.GetBillingDetailsWithNamesAsync())
                .ThrowsAsync(new Exception("Database error"));

            var exception = Assert.ThrowsAsync<ApplicationException>(async () =>
                await _adminService.GetBillingDetailsWithNamesAsync());

            Assert.That(exception.Message, Is.EqualTo("An error occurred while retrieving billing details."));
            Assert.That(exception.InnerException, Is.InstanceOf<Exception>());
            Assert.That(exception.InnerException.Message, Is.EqualTo("Database error"));
            _mockAdminRepository.Verify(repo => repo.GetBillingDetailsWithNamesAsync(), Times.Once);
        }

        [Test]
        public async Task MarkBillAsPaidAsync_WhenBillingExistsAndNotPaid_ReturnsTrue()
        {
            var billingId = 1;
            var billing = new Billing { BillingID = billingId, Status = "Unpaid" };

            _mockAdminRepository
                .Setup(repo => repo.GetBillingByIdAsync(billingId))
                .ReturnsAsync(billing);

            _mockAdminRepository
                .Setup(repo => repo.UpdateBillingAsync(It.IsAny<Billing>()))
                .Returns(Task.CompletedTask);

            var result = await _adminService.MarkBillAsPaidAsync(billingId);

            Assert.That(result, Is.True);
            Assert.That(billing.Status, Is.EqualTo("Paid"));
            _mockAdminRepository.Verify(repo => repo.GetBillingByIdAsync(billingId), Times.Once);
            _mockAdminRepository.Verify(repo => repo.UpdateBillingAsync(It.IsAny<Billing>()), Times.Once);
        }
    }
}
