using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AmazeCareAPI.Dtos;
using System.Security.Claims;
using AmazeCareAPI.Services.Interface;
using AmazeCareAPI.Exceptions;

namespace AmazeCareAPI.Controllers
{
    [Authorize(Roles = "Patient")]
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }
        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsernameAvailability([FromQuery] string username)
        {
            try
            {
                var (isAvailable, message) = await _patientService.CheckUsernameAvailabilityAsync(username);
                return Ok(new { Username = username, IsAvailable = isAvailable, Message = message });
            }
            catch (ServiceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while checking username availability.");
            }
        }

        [HttpGet("GetPersonalInfo")]
        public async Task<IActionResult> GetPersonalInfo()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var personalInfo = await _patientService.GetPersonalInfoAsync(userId);

                if (personalInfo == null)
                    return NotFound("User not found.");

                return Ok(personalInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving personal information.");
            }
        }


        [HttpPut("UpdatePersonalInfo")]
        public async Task<IActionResult> UpdatePersonalInfo([FromBody] UpdatePersonalInfoDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Invalid details provided.");

                int? userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized("User ID not found in token.");

                var (isSuccess, message) = await _patientService.UpdatePersonalInfoAsync(userId.Value, updateDto);

                if (!isSuccess)
                    return BadRequest(message);

                return Ok(message);
            }
            catch (ServiceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating personal information.");
            }
        }


        [HttpGet("SearchDoctors")]
        public async Task<IActionResult> SearchDoctors([FromQuery] string? specialization = null)
        {
            try
            {
                var doctors = await _patientService.SearchDoctors(specialization);

                if (doctors == null || !doctors.Any())
                    return NotFound("No doctors found for the specified specialization.");

                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while searching for doctors.");
            }
        }

        [HttpGet("DoctorSchedule/{doctorId}")]
        public async Task<IActionResult> GetDoctorSchedule(int doctorId)
        {
            try
            {
                var schedule = await _patientService.GetDoctorScheduleAsync(doctorId);

                if (schedule == null || schedule.Count == 0)
                    return NotFound("No schedule found for the specified doctor.");

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the doctor's schedule.");
            }
        }

        [HttpPost("ScheduleAppointment")]
        public async Task<IActionResult> ScheduleAppointment(AppointmentBookingDto bookingDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var (appointment, message) = await _patientService.ScheduleAppointment(userId, bookingDto);

                if (appointment == null)
                    return BadRequest(message);

                return Ok(new { appointment, message });
            }
            catch (ServiceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while scheduling the appointment.");
            }
        }


        
        [HttpPut("RescheduleAppointment/{appointmentId}")]
        public async Task<IActionResult> RescheduleAppointment(int appointmentId, [FromBody] AppointmentRescheduleDto rescheduleDto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized("User not authenticated.");

                var (appointment, message) = await _patientService.RescheduleAppointment(userId.Value, appointmentId, rescheduleDto);

                if (appointment == null)
                    return BadRequest(message);

                return Ok(new { appointment, message });
            }
            catch (ServiceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while rescheduling the appointment.");
            }
        }




        [HttpGet("GetMedicalHistory")]
        public async Task<IActionResult> GetMedicalHistory()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized("User ID not found in token.");

                var medicalHistory = await _patientService.GetMedicalHistory(userId.Value);

                if (medicalHistory == null || !medicalHistory.Any())
                    return NotFound("No medical history found.");

                return Ok(medicalHistory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving medical history.");
            }
        }


        [HttpGet("GetTestDetails")]
        public async Task<IActionResult> GetTestDetails()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized("User ID not found in token.");

                var testDetails = await _patientService.GetTestDetails(userId.Value);

                if (testDetails == null || !testDetails.Any())
                    return NotFound("No test details found.");

                return Ok(testDetails);
            }
            catch (ServiceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving test details.");
            }
        }


        [HttpGet("GetPrescriptionDetails")]
        public async Task<IActionResult> GetPrescriptionDetails()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized("User ID not found in token.");

                var prescriptionDetails = await _patientService.GetPrescriptionDetails(userId.Value);

                if (prescriptionDetails == null || !prescriptionDetails.Any())
                    return NotFound("No prescription details found.");

                return Ok(prescriptionDetails);
            }
            catch (ServiceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving prescription details.");
            }
        }

        [HttpGet("GetBillingDetails")]
        public async Task<IActionResult> GetBillingDetails()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                    return Unauthorized("User ID not found in token.");

                var billingDetails = await _patientService.GetBillingDetails(userId.Value);
                return Ok(billingDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving billing details.");
            }
        }
        [HttpGet("GetAppointments")]
        public async Task<IActionResult> GetAppointments()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var appointments = await _patientService.GetAppointments(userId);

                if (appointments == null || !appointments.Any())
                    return NotFound("No appointments found.");

                return Ok(appointments);
            }
            catch (ServiceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving appointments.");
            }
        }
        [HttpPost("CancelAppointment/{appointmentId}")]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _patientService.CancelAppointment(userId, appointmentId);

                if (!result)
                    return BadRequest("Unable to cancel the appointment.");

                return Ok("Appointment canceled successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while canceling the appointment.");
            }
        }

        [HttpGet("PatientBills")]
        public async Task<IActionResult> GetPatientBills()
        {
            var userId = GetUserIdFromToken(); 
            if (userId == null)
                return Unauthorized("User not found.");

            var bills = await _patientService.GetBillsByPatientIdAsync(userId.Value);
            return Ok(bills);
        }



        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : (int?)null;
        }
    }
}
