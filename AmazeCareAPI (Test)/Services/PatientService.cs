
using AmazeCareAPI.Data;
using AmazeCareAPI.Dtos;
using AmazeCareAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmazeCareAPI.Services.Interface;
using AmazeCareAPI.Repositories.Interface;
using AmazeCareAPI.Exceptions;

namespace AmazeCareAPI.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }
        public async Task<(bool IsAvailable, string Message)> CheckUsernameAvailabilityAsync(string username)
        {
            try
            {
                bool isAvailable = await _patientRepository.IsUsernameAvailableAsync(username);
                string message = isAvailable
                    ? "Username is available."
                    : "Username is already taken. Please choose a different username.";

                return (isAvailable, message);
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while checking username availability.", ex);
            }
        }
        public async Task<PatientPersonalInfoDto?> GetPersonalInfoAsync(int userId)
        {
            try
            {
                var user = await _patientRepository.GetUserByIdAsync(userId)
                            ?? throw new UserNotFoundException($"User with ID {userId} not found.");
                var patient = await _patientRepository.GetPatientByUserIdAsync(userId)
                             ?? throw new PatientNotFoundException($"Patient record for user ID {userId} not found.");

                return new PatientPersonalInfoDto
                {
                    UserId = user.UserID,
                    Username = user.Username,
                    FullName = patient.FullName,
                    Email = patient.Email,
                    DateOfBirth = patient.DateOfBirth,
                    Gender = patient.Gender,
                    ContactNumber = patient.ContactNumber,
                    Address = patient.Address,
                    MedicalHistory = patient.MedicalHistory
                };
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while retrieving personal information.", ex);
            }
        }
        public async Task<(bool IsSuccess, string Message)> UpdatePersonalInfoAsync(int userId, UpdatePersonalInfoDto updateDto)
        {
            try
            {
                var user = await _patientRepository.GetUserByIdAsync(userId)
                            ?? throw new UserNotFoundException("User not found.");

                if (!string.Equals(user.Username, updateDto.Username, StringComparison.OrdinalIgnoreCase))
                {
                    var (isAvailable, message) = await CheckUsernameAvailabilityAsync(updateDto.Username);
                    if (!isAvailable) return (false, message);
                }

                user.Username = updateDto.Username;

                if (!string.IsNullOrWhiteSpace(updateDto.NewPassword))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateDto.NewPassword);
                }

                await _patientRepository.UpdateUserAsync(user);

                var patient = await _patientRepository.GetPatientByUserIdAsync(userId)
                             ?? throw new PatientNotFoundException("Patient record not found.");

                patient.FullName = updateDto.FullName;
                patient.Email = updateDto.Email;
                patient.ContactNumber = updateDto.ContactNumber;
                patient.Address = updateDto.Address;
                patient.MedicalHistory = updateDto.MedicalHistory;
                patient.DateOfBirth = updateDto.DateOfBirth;
                patient.Gender = updateDto.Gender;

                await _patientRepository.UpdatePatientAsync(patient);
                await _patientRepository.SaveChangesAsync();

                return (true, "Personal information updated successfully.");
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while updating personal information.", ex);
            }
        }

        //Appointment
        public async Task<(Appointment appointment, string message)> ScheduleAppointment(int userId, AppointmentBookingDto bookingDto)
        {
            try
            {
                var patient = await _patientRepository.GetPatientByUserIdAsync(userId)
                             ?? throw new PatientNotFoundException("Patient not found.");

                bool isOnSchedule = await _patientRepository.IsDoctorOnSchedule(bookingDto.DoctorID, bookingDto.AppointmentDate);
                if (!isOnSchedule)
                    throw new InvalidOperationException("The selected appointment date and time doctor is not available.");

                var appointment = new Appointment
                {
                    PatientID = patient.PatientID,
                    DoctorID = bookingDto.DoctorID,
                    AppointmentDate = bookingDto.AppointmentDate,
                    Symptoms = bookingDto.Symptoms,
                    Status = "Requested"
                };

                await _patientRepository.AddAppointmentAsync(appointment);
                await _patientRepository.SaveChangesAsync();

                return (appointment, "Appointment requested successfully.");
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while scheduling an appointment.", ex);
            }
        }

        // Medical records
        public async Task<List<PatientMedicalRecordDto>> GetMedicalHistory(int userId)
        {
            try
            {
                var patient = await _patientRepository.GetPatientByUserIdAsync(userId)
                             ?? throw new PatientNotFoundException("Patient not found.");

                return await _patientRepository.GetMedicalHistoryAsync(patient.PatientID);
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while retrieving medical history.", ex);
            }
        }
        // Serach Doctor using specilaization
        public async Task<IEnumerable<DoctorDto>> SearchDoctors(string specialization = null)
        {
            try
            {
                return await _patientRepository.SearchDoctorsAsync(specialization);
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while searching for doctors.", ex);
            }
        }
        // get all appointment
        public async Task<IEnumerable<AppointmentWithDoctorDto>> GetAppointments(int userId)
        {
            try
            {
                var patient = await _patientRepository.GetPatientByUserIdAsync(userId)
                             ?? throw new PatientNotFoundException("Patient not found.");

                return await _patientRepository.GetAppointmentsByPatientIdAsync(patient.PatientID);
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while retrieving appointments.", ex);
            }
        }
        //cancel appointment
        public async Task<bool> CancelAppointment(int userId, int appointmentId)
        {
            try
            {
                var patient = await _patientRepository.GetPatientByUserIdAsync(userId)
                             ?? throw new PatientNotFoundException("Patient not found.");

                var appointment = await _patientRepository.GetAppointmentByIdAsync(patient.PatientID, appointmentId)
                                ?? throw new AppointmentNotFoundException("Appointment not found or already canceled.");

                if (appointment.Status == "Canceled")
                    throw new InvalidOperationException("Appointment is already canceled.");

                appointment.Status = "Canceled";
                await _patientRepository.UpdateAppointmentAsync(appointment);
                await _patientRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while canceling the appointment.", ex);
            }
        }

        // get test details
        public async Task<List<PatientTestDetailDto>> GetTestDetails(int userId)
        {
            try
            {
                var patientId = await _patientRepository.GetPatientIdByUserIdAsync(userId);
                return await _patientRepository.GetTestDetailsByPatientIdAsync(patientId);
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while retrieving test details.", ex);
            }
        }
        // get prescriptons
        public async Task<List<PatientPrescriptionDetailDto>> GetPrescriptionDetails(int userId)
        {
            try
            {
                var patientId = await _patientRepository.GetPatientIdByUserIdAsync(userId);
                return await _patientRepository.GetPrescriptionDetailsByPatientIdAsync(patientId);
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while retrieving prescription details.", ex);
            }
        }
        // get bills
        public async Task<List<BillingDto>> GetBillingDetails(int userId)
        {
            try
            {
                var patientId = await _patientRepository.GetPatientIdByUserIdAsync(userId);
                return await _patientRepository.GetBillingDetailsByPatientIdAsync(patientId);
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while retrieving billing details.", ex);
            }
        }
        // reschedule appointment 
        public async Task<(Appointment? appointment, string message)> RescheduleAppointment(int userId, int appointmentId, AppointmentRescheduleDto rescheduleDto)
        {
            try
            {
                var patientId = await _patientRepository.GetPatientIdByUserIdAsync(userId);
                var appointment = await _patientRepository.GetAppointmentByIdAndPatientIdAsync(appointmentId, patientId)
                                ?? throw new AppointmentNotFoundException("Appointment not found or unauthorized access.");

                if (appointment.Status != "Requested" && appointment.Status != "Scheduled")
                    throw new InvalidOperationException("Only requested or scheduled appointments can be rescheduled.");

                var isOnSchedule = await _patientRepository.IsDoctorOnScheduleAsync(appointment.DoctorID, rescheduleDto.NewAppointmentDate);
                if (!isOnSchedule)
                    throw new InvalidOperationException("The doctor is not available on the chosen date.");

                appointment.AppointmentDate = rescheduleDto.NewAppointmentDate;
                appointment.Status = "Requested";

                await _patientRepository.UpdateAppointmentAsync(appointment);
                await _patientRepository.SaveChangesAsync();

                return (appointment, "Appointment rescheduled successfully and status updated to 'Requested'.");
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while rescheduling the appointment.", ex);
            }
        }

        public async Task<List<ScheduleDto>> GetDoctorScheduleAsync(int doctorId)
        {
            try
            {
                var schedule = await _patientRepository.GetDoctorScheduleAsync(doctorId)
                              ?? throw new DoctorNotFoundException($"Doctor with ID {doctorId} not found or no schedule available.");

                return schedule.Select(h => new ScheduleDto
                {
                    ScheduleID = h.ScheduleID,
                    DoctorID = h.DoctorID,
                    StartDate = h.StartDate,
                    EndDate = h.EndDate,
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while retrieving the doctor's schedule.", ex);
            }
        }
        public async Task<IEnumerable<PatientBillingDetailsDto>> GetBillsByPatientIdAsync(int userId)
        {
            try
            {
                var patientId = await _patientRepository.GetPatientIdByUserIdAsync(userId);

                var bills = await _patientRepository.GetBillsByPatientIdAsync(patientId);

                if (bills == null || !bills.Any())
                    throw new BillingNotFoundException($"No billing details found for patient ID {patientId}.");

                return bills.Select(b => new PatientBillingDetailsDto
                {
                    BillingID = b.BillingID,
                    PatientID = b.PatientID,
                    PatientName = b.Patient.FullName,
                    DoctorID = b.Doctor.DoctorID,
                    DoctorName = b.Doctor.FullName,
                    ConsultationFee = b.ConsultationFee,
                    TotalTestsPrice = b.TotalTestsPrice,
                    TotalMedicationsPrice = b.TotalMedicationsPrice,
                    GrandTotal = b.GrandTotal,
                    Status = b.Status
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new ServiceException("An error occurred while retrieving billing details for the patient.", ex);
            }
        }

    }
}
