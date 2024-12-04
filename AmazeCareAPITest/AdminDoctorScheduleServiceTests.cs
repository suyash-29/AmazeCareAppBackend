using AmazeCareAPI.Dtos;
using AmazeCareAPI.Models;
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
    public class AdminDoctorScheduleServiceTests
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
        public async Task GetSchedulesWithDoctorNameAsync_WhenSchedulesExist_ReturnsSchedulesWithDoctorDetails()
        {
            var doctorId = 1;
            var doctorSchedules = new List<DoctorSchedule>
            {
                new DoctorSchedule
                {
                    ScheduleID = 1,
                    DoctorID = doctorId,
                    StartDate = DateTime.Now.AddHours(1),
                    EndDate = DateTime.Now.AddHours(2),
                    Status = "Scheduled",
                    Doctor = new Doctor { DoctorID = doctorId, FullName = "Dr. John Doe" }
                }
            };

            _mockAdminRepository.Setup(repo => repo.GetSchedulesWithDoctorDetailsAsync(doctorId))
                .ReturnsAsync(doctorSchedules);

            var result = await _adminService.GetSchedulesWithDoctorNameAsync(doctorId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().DoctorName, Is.EqualTo("Dr. John Doe"));
            Assert.That(result.First().ScheduleID, Is.EqualTo(1));
            Assert.That(result.First().Status, Is.EqualTo("Scheduled"));
        }

        [Test]
        public void GetSchedulesWithDoctorNameAsync_WhenNoSchedulesFound_ThrowsApplicationException()
        {
            var doctorId = 1;
            _mockAdminRepository.Setup(repo => repo.GetSchedulesWithDoctorDetailsAsync(doctorId))
                .ReturnsAsync((IEnumerable<DoctorSchedule>)null); 

            var exception = Assert.ThrowsAsync<ApplicationException>(async () =>
                await _adminService.GetSchedulesWithDoctorNameAsync(doctorId));

            Assert.That(exception.Message, Is.EqualTo("An error occurred while retrieving schedules with doctor details."));
            Assert.That(exception.InnerException, Is.InstanceOf<KeyNotFoundException>());
        }

        [Test]
        public async Task UpdateScheduleByAdminAsync_WhenScheduleExists_UpdatesScheduleSuccessfully()
        {
            var scheduleId = 1;
            var updateDto = new UpdateScheduleDto
            {
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(2)
            };

            var schedule = new DoctorSchedule
            {
                ScheduleID = scheduleId,
                DoctorID = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Status = "Scheduled"
            };

            _mockAdminRepository.Setup(repo => repo.GetScheduleByIdAsync(scheduleId)).ReturnsAsync(schedule);
            _mockAdminRepository.Setup(repo => repo.UpdateScheduleAsync(It.IsAny<DoctorSchedule>())).Returns(Task.CompletedTask);

            var result = await _adminService.UpdateScheduleByAdminAsync(scheduleId, updateDto);

            Assert.That(result, Is.True);
            Assert.That(schedule.StartDate, Is.EqualTo(updateDto.StartDate));
            Assert.That(schedule.EndDate, Is.EqualTo(updateDto.EndDate));
            Assert.That(schedule.Status, Is.EqualTo("Scheduled"));
            _mockAdminRepository.Verify(repo => repo.UpdateScheduleAsync(It.IsAny<DoctorSchedule>()), Times.Once);
        }

        [Test]
        public async Task CancelScheduleByAdminAsync_WhenScheduleExists_CancelsScheduleSuccessfully()
        {
            var scheduleId = 1;
            var schedule = new DoctorSchedule
            {
                ScheduleID = scheduleId,
                DoctorID = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Status = "Scheduled"
            };

            _mockAdminRepository.Setup(repo => repo.GetScheduleByIdAsync(scheduleId)).ReturnsAsync(schedule);
            _mockAdminRepository.Setup(repo => repo.UpdateScheduleAsync(It.IsAny<DoctorSchedule>())).Returns(Task.CompletedTask);

            var result = await _adminService.CancelScheduleByAdminAsync(scheduleId);

            Assert.That(result, Is.True);
            Assert.That(schedule.Status, Is.EqualTo("Cancelled"));
            _mockAdminRepository.Verify(repo => repo.UpdateScheduleAsync(It.IsAny<DoctorSchedule>()), Times.Once);
        }

        [Test]
        public async Task CancelScheduleByAdminAsync_WhenScheduleIsCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var scheduleId = 1;
            var schedule = new DoctorSchedule
            {
                ScheduleID = scheduleId,
                DoctorID = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Status = "Completed"
            };

            _mockAdminRepository.Setup(repo => repo.GetScheduleByIdAsync(scheduleId)).ReturnsAsync(schedule);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(async () =>
                await _adminService.CancelScheduleByAdminAsync(scheduleId));

            Assert.That(exception.InnerException, Is.InstanceOf<InvalidOperationException>());
            Assert.That(exception.InnerException.Message, Is.EqualTo("Cannot cancel completed or non-existent schedule."));
        }

        [Test]
        public async Task MarkScheduleAsCompletedAsync_WhenScheduleIsCancelled_ThrowsInvalidOperationException()
        {
            // Arrange
            var scheduleId = 1;
            var schedule = new DoctorSchedule
            {
                ScheduleID = scheduleId,
                DoctorID = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Status = "Cancelled" 
            };

            _mockAdminRepository.Setup(repo => repo.GetScheduleByIdAsync(scheduleId)).ReturnsAsync(schedule);

            var exception = Assert.ThrowsAsync<ApplicationException>(async () =>
                await _adminService.MarkScheduleAsCompletedAsync(scheduleId));

            Assert.That(exception.InnerException, Is.InstanceOf<InvalidOperationException>());
            Assert.That(exception.InnerException.Message, Is.EqualTo("Cannot mark cancelled or non-existent schedule as completed."));
        }

        [Test]
        public async Task MarkScheduleAsCompletedAsync_WhenScheduleIsValid_ChangesStatusToCompleted()
        {
            var scheduleId = 1;
            var schedule = new DoctorSchedule
            {
                ScheduleID = scheduleId,
                DoctorID = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Status = "Scheduled" 
            };

            _mockAdminRepository.Setup(repo => repo.GetScheduleByIdAsync(scheduleId)).ReturnsAsync(schedule);
            _mockAdminRepository.Setup(repo => repo.UpdateScheduleAsync(It.IsAny<DoctorSchedule>())).Returns(Task.CompletedTask);

            var result = await _adminService.MarkScheduleAsCompletedAsync(scheduleId);

            Assert.That(result, Is.True);
            Assert.That(schedule.Status, Is.EqualTo("Completed"));
        }
    }
}
