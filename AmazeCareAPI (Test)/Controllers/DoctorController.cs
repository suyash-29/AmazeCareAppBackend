using AmazeCareAPI.Data;
using AmazeCareAPI.Dtos;
using AmazeCareAPI.Models;
using AmazeCareAPI.Services;
using AmazeCareAPI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Xml.Serialization;
using AmazeCareAPI.Exceptions;

namespace AmazeCareAPI.Controllers
{

    [Authorize(Roles = "Doctor")]
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        //private readonly AmazeCareContext _context;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet("GetAppointmentsByStatus")]
        public async Task<IActionResult> GetAppointmentsByStatus([FromQuery] string status)
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                if (string.IsNullOrWhiteSpace(status) || !new[] { "Completed", "Scheduled", "Canceled", "Requested" }.Contains(status))
                    return BadRequest("Invalid status. Valid values are 'Completed', 'Scheduled', 'Canceled', or 'Requested'.");

                var appointments = await _doctorService.GetAppointmentsByStatus(doctorId.Value, status);
                if (appointments == null || !appointments.Any())
                    return NotFound("No appointments found with the specified status.");

                return Ok(appointments);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception here if needed
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
        [HttpPut("ApproveAppointment/{appointmentId}/approve")]
        public async Task<IActionResult> ApproveAppointmentRequest(int appointmentId)
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                var success = await _doctorService.ApproveAppointmentRequest(doctorId.Value, appointmentId);
                if (!success)
                    return NotFound("Appointment not found or it is not in a requested state.");

                return Ok("Appointment approved successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }



        [HttpPut("CancelAppointment/{appointmentId}/cancel")]
        public async Task<IActionResult> CancelScheduledAppointment(int appointmentId)
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                var success = await _doctorService.CancelScheduledAppointment(doctorId.Value, appointmentId);
                if (!success)
                    return NotFound("Appointment not found or it is not scheduled.");

                return Ok("Appointment canceled successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }


        [HttpPut("RescheduleAppointment/{appointmentId}")]
        public async Task<IActionResult> RescheduleAppointment(int appointmentId, [FromBody] AppointmentRescheduleDto rescheduleDto)
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                if (doctorId == null)
                    return Unauthorized("Doctor ID not found for the authenticated user.");

                var (success, message) = await _doctorService.RescheduleAppointmentAsync(doctorId.Value, appointmentId, rescheduleDto);

                if (!success)
                    return BadRequest(message);

                return Ok("Appointment rescheduled successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("appointments/{appointmentId}/consult")]
        public async Task<IActionResult> ConductConsultation(int appointmentId, [FromBody] CreateMedicalRecordDto recordDto, [FromQuery] decimal consultationFee)
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                if (doctorId == null)
                    return Unauthorized("Doctor ID not found for the authenticated user.");

                var success = await _doctorService.ConductConsultation(doctorId.Value, appointmentId, recordDto, consultationFee);

                if (!success)
                    return BadRequest("Failed to conduct consultation.");

                return Ok("Consultation completed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // Get medical records 
        [HttpGet("GetPatientMedicalRecords/{patientId}/medical-records")]
        public async Task<IActionResult> GetPatientMedicalRecords(int patientId)
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                if (doctorId == null)
                    return Unauthorized("Doctor ID not found for the authenticated user.");

                var records = await _doctorService.GetMedicalRecordsByPatientIdAsync(patientId);
                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        // Update Medical Record
        [HttpPut("UpdateMedicalRecord/{recordId}/{patientId}")]
        public async Task<IActionResult> UpdateMedicalRecord(int recordId, int patientId, [FromBody] UpdateMedicalRecordDto recordDto)
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                if (doctorId == null)
                    return Unauthorized("Doctor ID not found for the authenticated user.");

                var success = await _doctorService.UpdateMedicalRecord(doctorId.Value, recordId, patientId, recordDto);
                if (!success)
                    return NotFound("Medical record not found or unauthorized access.");

                return Ok("Medical record updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("billing/{billingId}/pay")]
        public async Task<IActionResult> UpdateBillingStatus(int billingId)
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                if (doctorId == null)
                    return Unauthorized("Doctor ID not found for the authenticated user.");

                var success = await _doctorService.UpdateBillingStatusAsync(billingId, doctorId.Value);
                if (!success)
                    return BadRequest("Billing record not found or already marked as 'Paid'.");

                return Ok("Billing status updated to 'Paid'.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("GetBills")]
        public async Task<IActionResult> GetDoctorBills()
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                if (doctorId == null)
                    return Unauthorized("Doctor not authenticated.");

                var bills = await _doctorService.GetBillsByDoctorIdAsync(doctorId.Value);
                return Ok(bills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("GetAllMedications")]
        public async Task<ActionResult<IEnumerable<MedicationDto>>> GetMedications()
        {
            try
            {
                var medications = await _doctorService.GetMedicationsAsync();
                return Ok(medications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("GetAllTests")]
        public async Task<ActionResult<IEnumerable<TestDto>>> GetTests()
        {
            try
            {
                var tests = await _doctorService.GetTestsAsync();
                return Ok(tests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("CreateSchedule")]
        public async Task<IActionResult> AddSchedule([FromBody] CreateScheduleDto scheduleDto)
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                if (doctorId == null)
                    return Unauthorized("Doctor ID not found for the authenticated user.");

                bool success = await _doctorService.AddScheduleAsync(doctorId.Value, scheduleDto);
                if (!success)
                    return BadRequest("Failed to add schedule.");

                return Ok("Schedule added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("UpdateSchedule/{scheduleId}")]
        public async Task<IActionResult> UpdateSchedule(int scheduleId, [FromBody] UpdateScheduleDto updateDto)
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                if (doctorId == null)
                    return Unauthorized("Doctor ID not found for the authenticated user.");

                bool success = await _doctorService.UpdateScheduleAsync(scheduleId, doctorId.Value, updateDto);
                if (!success)
                    return NotFound("Schedule not found or unauthorized access.");

                return Ok("Schedule updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("GetALLSchedules")]
        public async Task<IActionResult> GetSchedules()
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                if (doctorId == null)
                    return Unauthorized("Doctor ID not found for the authenticated user.");

                var schedules = await _doctorService.GetSchedulesAsync(doctorId.Value);
                if (!schedules.Any())
                    return NotFound("No Schedule found.");

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("CancelSchedule/{scheduleId}")]
        public async Task<IActionResult> CancelSchedule(int scheduleId)
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                if (doctorId == null)
                    return Unauthorized("Doctor ID not found for the authenticated user.");

                bool success = await _doctorService.CancelScheduleAsync(scheduleId, doctorId.Value);
                if (!success)
                    return NotFound("Schedule not found, unauthorized access, or already completed.");

                return Ok("Schedule cancelled successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized($"Unauthorized access: {ex.Message}");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound($"Schedule not found: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Invalid operation: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpPut("CompleteSchedules/{scheduleId}")]
        public async Task<IActionResult> CompleteSchedule(int scheduleId)
        {
            try
            {
                var doctorId = await GetDoctorIdFromTokenAsync();
                if (doctorId == null)
                    return Unauthorized("Doctor ID not found for the authenticated user.");

                bool success = await _doctorService.CompleteScheduleAsync(scheduleId, doctorId.Value);
                if (!success)
                    return NotFound("Schedule not found, unauthorized access, or already completed.");

                return Ok("Marked Completed successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized($"Unauthorized access: {ex.Message}");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound($"Schedule not found: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Invalid operation: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }



        private async Task<int?> GetDoctorIdFromTokenAsync()
        {
            // get userID from token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            // doctorid form usrid use
            var DoctorID = await _doctorService.GetDoctorIdAsync(userId);
            //var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == userId);
            return DoctorID;
        }

    }

}

