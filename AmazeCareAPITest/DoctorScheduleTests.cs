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
    internal class DoctorScheduleTests
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
        public async Task AddScheduleAsync_ShouldReturnTrue_WhenScheduleIsValid()
        {
            int doctorId = 1;
            var scheduleDto = new CreateScheduleDto
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1)
            };

            _mockDoctorRepository.Setup(repo => repo.AddScheduleAsync(It.IsAny<DoctorSchedule>()))
                .Returns(Task.CompletedTask);

            var result = await _doctorService.AddScheduleAsync(doctorId, scheduleDto);

            Assert.That(result, Is.True);
            _mockDoctorRepository.Verify(repo => repo.AddScheduleAsync(It.Is<DoctorSchedule>(s => s.DoctorID == doctorId)), Times.Once);
        }

        [Test]
        public async Task UpdateScheduleAsync_ShouldReturnTrue_WhenScheduleExists()
        {
            int scheduleId = 1;
            int doctorId = 1;
            var updateDto = new UpdateScheduleDto
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Status = "Updated"
            };

            var existingSchedule = new DoctorSchedule
            {
                ScheduleID = scheduleId,
                DoctorID = doctorId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Status = "Scheduled"
            };

            _mockDoctorRepository.Setup(repo => repo.GetScheduleByIdAndDoctorIdAsync(scheduleId, doctorId))
                .ReturnsAsync(existingSchedule);

            _mockDoctorRepository.Setup(repo => repo.UpdateScheduleAsync(It.IsAny<DoctorSchedule>()))
                .Returns(Task.CompletedTask);

            var result = await _doctorService.UpdateScheduleAsync(scheduleId, doctorId, updateDto);

            Assert.That(result, Is.True);
            _mockDoctorRepository.Verify(repo => repo.UpdateScheduleAsync(It.Is<DoctorSchedule>(s => s.Status == updateDto.Status)), Times.Once);
        }

        [Test]
        public async Task GetSchedulesAsync_ShouldReturnSchedules_WhenSchedulesExist()
        {
            int doctorId = 1;
            var schedules = new List<ScheduleDto>
            {
                new ScheduleDto { ScheduleID = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1), Status = "Scheduled" }
            };

            _mockDoctorRepository.Setup(repo => repo.GetSchedulesByDoctorIdAsync(doctorId))
                .ReturnsAsync(schedules);

            var result = await _doctorService.GetSchedulesAsync(doctorId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            _mockDoctorRepository.Verify(repo => repo.GetSchedulesByDoctorIdAsync(doctorId), Times.Once);
        }

        [Test]
        public async Task CancelScheduleAsync_ShouldReturnTrue_WhenScheduleCanBeCancelled()
        {
            int scheduleId = 1;
            int doctorId = 1;

            var schedule = new DoctorSchedule
            {
                ScheduleID = scheduleId,
                DoctorID = doctorId,
                Status = "Scheduled"
            };

            _mockDoctorRepository.Setup(repo => repo.GetScheduleByIdAndDoctorIdAsync(scheduleId, doctorId))
                .ReturnsAsync(schedule);

            _mockDoctorRepository.Setup(repo => repo.UpdateScheduleAsync(It.IsAny<DoctorSchedule>()))
                .Returns(Task.CompletedTask);

            var result = await _doctorService.CancelScheduleAsync(scheduleId, doctorId);

            Assert.That(result, Is.True);
            _mockDoctorRepository.Verify(repo => repo.UpdateScheduleAsync(It.Is<DoctorSchedule>(s => s.Status == "Cancelled")), Times.Once);
        }

        [Test]
        public async Task CompleteScheduleAsync_ShouldReturnTrue_WhenScheduleCanBeCompleted()
        {
            int scheduleId = 1;
            int doctorId = 1;

            var schedule = new DoctorSchedule
            {
                ScheduleID = scheduleId,
                DoctorID = doctorId,
                Status = "Scheduled"
            };

            _mockDoctorRepository.Setup(repo => repo.GetScheduleByIdAndDoctorIdAsync(scheduleId, doctorId))
                .ReturnsAsync(schedule);

            _mockDoctorRepository.Setup(repo => repo.UpdateScheduleAsync(It.IsAny<DoctorSchedule>()))
                .Returns(Task.CompletedTask);

            var result = await _doctorService.CompleteScheduleAsync(scheduleId, doctorId);

            Assert.That(result, Is.True);
            _mockDoctorRepository.Verify(repo => repo.UpdateScheduleAsync(It.Is<DoctorSchedule>(s => s.Status == "Completed")), Times.Once);
        }
    }
}
