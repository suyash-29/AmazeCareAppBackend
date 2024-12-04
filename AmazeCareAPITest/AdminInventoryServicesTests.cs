using AmazeCareAPI.Repositories.Interface;
using AmazeCareAPI.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmazeCareAPI.Models;
using AmazeCareAPI.Dtos;

namespace AmazeCareAPITest
{
    [TestFixture]
    public class AdminInventoryServicesTests
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
        public async Task GetAllTestsAsync_WhenTestsExist_ReturnsTestDtos()
        {
            var testEntities = new List<Test>
            {
                new Test { TestID = 1, TestName = "Test 1", TestPrice = 100.00m },
                new Test { TestID = 2, TestName = "Test 2", TestPrice = 200.00m }
            };

            _mockAdminRepository.Setup(repo => repo.GetAllTestsAsync()).ReturnsAsync(testEntities);

            var result = await _adminService.GetAllTestsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().TestName, Is.EqualTo("Test 1"));
            Assert.That(result.First().TestPrice, Is.EqualTo(100.00m));
        }

        [Test]
        public async Task GetAllTestsAsync_WhenNoTestsFound_ThrowsKeyNotFoundException()
        {
            _mockAdminRepository.Setup(repo => repo.GetAllTestsAsync()).ReturnsAsync((List<Test>)null);

            var exception = Assert.ThrowsAsync<ApplicationException>(async () =>
                await _adminService.GetAllTestsAsync());

            Assert.That(exception.InnerException, Is.InstanceOf<KeyNotFoundException>());
            Assert.That(exception.InnerException.Message, Is.EqualTo("No tests found."));
        }

        [Test]
        public async Task AddTestAsync_WhenTestIsAddedSuccessfully_ReturnsTrue()
        {
            var createTestDto = new CreateUpdateTestDto
            {
                TestName = "Blood Test",
                TestPrice = 50.00m
            };

            _mockAdminRepository.Setup(repo => repo.AddTestAsync(It.IsAny<Test>())).Returns(Task.CompletedTask);

            var result = await _adminService.AddTestAsync(createTestDto);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task UpdateTestAsync_WhenTestIsUpdatedSuccessfully_ReturnsTrue()
        {
            var updateTestDto = new CreateUpdateTestDto
            {
                TestName = "Updated Blood Test",
                TestPrice = 55.00m
            };
            var existingTest = new Test
            {
                TestID = 1,
                TestName = "Blood Test",
                TestPrice = 50.00m
            };

            _mockAdminRepository.Setup(repo => repo.GetTestByIdAsync(It.IsAny<int>())).ReturnsAsync(existingTest);
            _mockAdminRepository.Setup(repo => repo.UpdateTestAsync(It.IsAny<Test>())).Returns(Task.CompletedTask);

            var result = await _adminService.UpdateTestAsync(1, updateTestDto);

            Assert.That(result, Is.True);
            _mockAdminRepository.Verify(repo => repo.UpdateTestAsync(It.Is<Test>(test =>
                test.TestName == updateTestDto.TestName && test.TestPrice == updateTestDto.TestPrice)), Times.Once);
        }

        [Test]
        public async Task GetAllMedicationsAsync_WhenMedicationsFound_ReturnsMedicationDtos()
        {
            var medications = new List<Medication>
             {
                new Medication { MedicationID = 1, MedicationName = "Aspirin", PricePerUnit = 10.0m },
                new Medication { MedicationID = 2, MedicationName = "Ibuprofen", PricePerUnit = 15.0m }
             };

            _mockAdminRepository.Setup(repo => repo.GetAllMedicationsAsync()).ReturnsAsync(medications);

            var result = await _adminService.GetAllMedicationsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().MedicationName, Is.EqualTo("Aspirin"));
            Assert.That(result.First().PricePerUnit, Is.EqualTo(10.0m));
        }

        [Test]
        public async Task AddMedicationAsync_WhenMedicationIsValid_ReturnsTrue()
        {
            var createMedicationDto = new CreateUpdateMedicationDto
            {
                MedicationName = "Paracetamol",
                PricePerUnit = 5.0m
            };

            _mockAdminRepository.Setup(repo => repo.AddMedicationAsync(It.IsAny<Medication>())).Returns(Task.CompletedTask);

            var result = await _adminService.AddMedicationAsync(createMedicationDto);

            Assert.That(result, Is.True);
            _mockAdminRepository.Verify(repo => repo.AddMedicationAsync(It.Is<Medication>(m =>
                m.MedicationName == "Paracetamol" && m.PricePerUnit == 5.0m)), Times.Once);
        }

        [Test]
        public async Task AddMedicationAsync_WhenExceptionOccurs_ReturnsApplicationException()
        {
            var createMedicationDto = new CreateUpdateMedicationDto
            {
                MedicationName = "Ibuprofen",
                PricePerUnit = 10.0m
            };

            _mockAdminRepository.Setup(repo => repo.AddMedicationAsync(It.IsAny<Medication>())).ThrowsAsync(new Exception("Database error"));

            var exception = Assert.ThrowsAsync<ApplicationException>(async () =>
                await _adminService.AddMedicationAsync(createMedicationDto));

            Assert.That(exception.Message, Is.EqualTo("An error occurred while adding a new medication."));
            Assert.That(exception.InnerException, Is.InstanceOf<Exception>());
        }

        [Test]
        public async Task UpdateMedicationAsync_WhenMedicationIsValid_ReturnsTrue()
        {
            var updateMedicationDto = new CreateUpdateMedicationDto
            {
                MedicationName = "Updated Ibuprofen",
                PricePerUnit = 12.5m
            };

            var existingMedication = new Medication
            {
                MedicationID = 1,
                MedicationName = "Ibuprofen",
                PricePerUnit = 10.0m
            };

            _mockAdminRepository.Setup(repo => repo.GetMedicationByIdAsync(1)).ReturnsAsync(existingMedication);
            _mockAdminRepository.Setup(repo => repo.UpdateMedicationAsync(It.IsAny<Medication>())).Returns(Task.CompletedTask);

            var result = await _adminService.UpdateMedicationAsync(1, updateMedicationDto);

            Assert.That(result, Is.True);
            _mockAdminRepository.Verify(repo => repo.UpdateMedicationAsync(It.Is<Medication>(m =>
                m.MedicationName == "Updated Ibuprofen" && m.PricePerUnit == 12.5m)), Times.Once);
        }
    }
}
