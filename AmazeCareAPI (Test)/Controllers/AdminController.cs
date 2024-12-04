// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AmazeCareAPI.Services.Interface;
using AmazeCareAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using AmazeCareAPI.Models;

namespace AmazeCareAPI.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("CheckUsername")]
        public async Task<IActionResult> CheckUsernameAvailability([FromQuery] string username)
        {
            try
            {
                var (isAvailable, message) = await _adminService.CheckUsernameAvailabilityAsync(username);
                return Ok(new { Username = username, IsAvailable = isAvailable, Message = message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking username availability.", error = ex.Message });
            }
        }



        [HttpPost("RegisterAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegistrationDto registrationDto)
        {
            try
            {
                var admin = await _adminService.RegisterAdmin(
                    registrationDto.Username,
                    registrationDto.Password,
                    registrationDto.FullName,
                    registrationDto.Email
                );
                return CreatedAtAction(nameof(RegisterAdmin), new { id = admin.AdminID }, admin);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while registering admin.", error = ex.Message });
            }
        }

        [HttpPost("RegisterDoctor")]
        public async Task<IActionResult> RegisterDoctor([FromBody] DoctorRegistrationDto doctorDto)
        {
            try
            {
                var doctor = await _adminService.RegisterDoctor(doctorDto);
                return Ok(new { message = "Doctor registered successfully.", doctor });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while registering doctor.", error = ex.Message });
            }
        }

        [HttpGet("specializations")]
        public async Task<IActionResult> GetAllSpecializations()
        {
            try
            {
                var specializations = await _adminService.GetAllSpecializationsAsync();
                if (specializations == null || !specializations.Any())
                    return NotFound("No specializations found.");
                return Ok(specializations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving specializations.", error = ex.Message });
            }
        }

        [HttpGet("GetDoctorDetails/{doctorId}")]
        public async Task<IActionResult> GetDoctorDetails(int doctorId)
        {
            try
            {
                var doctor = await _adminService.GetDoctorDetails(doctorId);
                if (doctor == null)
                    return NotFound($"Doctor with ID {doctorId} not found.");
                return Ok(doctor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving doctor details.", error = ex.Message });
            }
        }


        [HttpPut("UpdateDoctor/{doctorId}")]
        public async Task<IActionResult> UpdateDoctor(int doctorId, [FromBody] DoctorUpdateDto doctorDto)
        {
            try
            {
                var success = await _adminService.UpdateDoctorDetails(doctorId, doctorDto);
                if (!success)
                    return NotFound(new { message = "Doctor not found." });
                return Ok(new { message = "Doctor updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating doctor.", error = ex.Message });
            }
        }

        [HttpDelete("DeleteDoctor/{userId}/{doctorId}")]
        public async Task<IActionResult> DeleteDoctor(int userId, int doctorId)
        {
            try
            {
                var result = await _adminService.DeleteDoctor(userId, doctorId);
                if (!result)
                    return NotFound(new { message = "Doctor not found or invalid user ID." });
                return Ok(new { message = "Doctor deleted successfully. Associated appointments canceled." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting doctor.", error = ex.Message });
            }
        }


        [HttpGet("GetPatientDetails/{patientId}")]
        public async Task<IActionResult> GetPatientDetails(int patientId)
        {
            try
            {
                var patient = await _adminService.GetPatientDetails(patientId);
                return Ok(patient);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving patient details.", error = ex.Message });
            }
        }
        [HttpPost("UpdatePatient")]
        public async Task<IActionResult> UpdatePatient([FromBody] PatientDto patientDto)
        {
            try
            {
                var patient = await _adminService.UpdatePatient(patientDto);
                return Ok(patient);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating patient.", error = ex.Message });
            }
        }

        [HttpDelete("DeletePatient/{userId}/{patientId}")]
        public async Task<IActionResult> DeletePatient(int userId, int patientId)
        {
            try
            {
                var result = await _adminService.DeletePatient(userId, patientId);
                if (!result)
                    return NotFound(new { message = "Patient not found or invalid user ID." });
                return Ok(new { message = "Patient deleted successfully. Associated appointments canceled." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting patient.", error = ex.Message });
            }
        }


        [HttpGet("ViewAppointmentDetails/{appointmentId}")]
        public async Task<IActionResult> ViewAppointmentDetails(int appointmentId)
        {
            try
            {
                var appointment = await _adminService.ViewAppointmentDetails(appointmentId);
                if (appointment == null)
                    return NotFound("Appointment not found");
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving appointment details.", error = ex.Message });
            }
        }

        [HttpPut("admin/RescheduleAppointment/{appointmentId}")]
        public async Task<IActionResult> RescheduleAppointment(int appointmentId, [FromBody] AppointmentRescheduleDto rescheduleDto)
        {
            try
            {
                var appointment = await _adminService.RescheduleAppointment(appointmentId, rescheduleDto);
                if (appointment == null)
                    return NotFound("Appointment not found.");
                return Ok(appointment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while rescheduling the appointment.", error = ex.Message });
            }
        }


        [HttpGet("DoctorSchedule/GetAll/{doctorId}")]
        public async Task<IActionResult> GetSchedulesWithDoctorName(int doctorId)
        {
            try
            {
                var schedules = await _adminService.GetSchedulesWithDoctorNameAsync(doctorId);
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("DoctorSchedule/Update/{scheduleId}")]
        public async Task<IActionResult> UpdateSchedule(int scheduleId, [FromBody] UpdateScheduleDto updateDto)
        {
            try
            {
                var success = await _adminService.UpdateScheduleByAdminAsync(scheduleId, updateDto);
                if (!success)
                    return NotFound("Schedule not found.");

                return Ok("Schedule updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("DoctorSchedule/{scheduleId}/cancel")]
        public async Task<IActionResult> CancelSchedule(int scheduleId)
        {
            try
            {
                var success = await _adminService.CancelScheduleByAdminAsync(scheduleId);
                if (!success)
                    return NotFound("Schedule not found or already completed.");

                return Ok("Schedule canceled successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("DoctorSchedule/{scheduleId}/completed")]
        public async Task<IActionResult> MarkScheduleAsCompleted(int scheduleId)
        {
            try
            {
                var success = await _adminService.MarkScheduleAsCompletedAsync(scheduleId);
                if (!success)
                    return NotFound("Schedule not found or it is already cancelled.");

                return Ok("Schedule marked as completed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAllBilling")]
        public async Task<IActionResult> GetBillingDetails()
        {
            try
            {
                var billingDetails = await _adminService.GetBillingDetailsWithNamesAsync();
                return Ok(billingDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("Billing/{billingId}/pay")]
        public async Task<IActionResult> MarkBillAsPaid(int billingId)
        {
            try
            {
                var success = await _adminService.MarkBillAsPaidAsync(billingId);
                if (!success)
                    return NotFound("Billing record not found or already marked as paid.");

                return Ok("Bill marked as paid successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAllMedications")]
        public async Task<IActionResult> GetAllMedications()
        {
            try
            {
                var medications = await _adminService.GetAllMedicationsAsync();
                return Ok(medications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("AddMedications")]
        public async Task<IActionResult> AddMedication([FromBody] CreateUpdateMedicationDto createMedicationDto)
        {
            try
            {
                await _adminService.AddMedicationAsync(createMedicationDto);
                return Ok("Medication added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateMedications/{medicationId}")]
        public async Task<IActionResult> UpdateMedication(int medicationId, [FromBody] CreateUpdateMedicationDto updateMedicationDto)
        {
            try
            {
                var success = await _adminService.UpdateMedicationAsync(medicationId, updateMedicationDto);
                if (!success)
                    return NotFound("Medication not found.");

                return Ok("Medication updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAllTests")]
        public async Task<IActionResult> GetAllTests()
        {
            try
            {
                var tests = await _adminService.GetAllTestsAsync();
                return Ok(tests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("AddTests")]
        public async Task<IActionResult> AddTest([FromBody] CreateUpdateTestDto createTestDto)
        {
            try
            {
                await _adminService.AddTestAsync(createTestDto);
                return Ok("Test added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("UpdateTests/{testId}")]
        public async Task<IActionResult> UpdateTest(int testId, [FromBody] CreateUpdateTestDto updateTestDto)
        {
            try
            {
                var success = await _adminService.UpdateTestAsync(testId, updateTestDto);
                if (!success)
                    return NotFound("Test not found.");

                return Ok("Test updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }








    }
}
