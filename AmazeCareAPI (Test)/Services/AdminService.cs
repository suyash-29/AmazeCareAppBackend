using AmazeCareAPI.Models;
using AmazeCareAPI.Data;
using Microsoft.EntityFrameworkCore;
using AmazeCareAPI.Dtos;
using AmazeCareAPI.Services.Interface;
using AmazeCareAPI.Repositories.Interface;

namespace AmazeCareAPI.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<(bool IsAvailable, string Message)> CheckUsernameAvailabilityAsync(string username)
        {
            try
            {
                bool isAvailable = await _adminRepository.IsUsernameAvailableAsync(username);
                string message = isAvailable ? "Username is available." : "Username is already taken.";
                return (isAvailable, message);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking username availability.", ex);
            }
        }

        public async Task<Administrator> RegisterAdmin(string username, string password, string fullName, string email)
        {
            try
            {
                var (isAvailable, message) = await CheckUsernameAvailabilityAsync(username);
                if (!isAvailable) throw new InvalidOperationException(message);

                var user = new User
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    RoleID = 3
                };
                await _adminRepository.CreateUserAsync(user);

                var admin = new Administrator
                {
                    UserID = user.UserID,
                    FullName = fullName,
                    Email = email
                };
                await _adminRepository.CreateAdminAsync(admin);

                return admin;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while registering the admin.", ex);
            }
        }

        public async Task<IEnumerable<SpecializationDto>> GetAllSpecializationsAsync()
        {
            try
            {
                var specializations = await _adminRepository.GetAllSpecializationsAsync();

                return specializations.Select(s => new SpecializationDto
                {
                    SpecializationID = s.SpecializationID,
                    SpecializationName = s.SpecializationName
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving specializations.", ex);
            }
        }


        public async Task<Doctor> RegisterDoctor(DoctorRegistrationDto doctorDto)
        {
            try
            {
                var (isAvailable, message) = await CheckUsernameAvailabilityAsync(doctorDto.Username);
                if (!isAvailable) throw new InvalidOperationException(message);

                var user = new User
                {
                    Username = doctorDto.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(doctorDto.Password),
                    RoleID = 2
                };
                await _adminRepository.CreateUserAsync(user);

                var doctor = new Doctor
                {
                    UserID = user.UserID,
                    FullName = doctorDto.FullName,
                    Email = doctorDto.Email,
                    ExperienceYears = doctorDto.ExperienceYears,
                    Qualification = doctorDto.Qualification,
                    Designation = doctorDto.Designation
                };
                await _adminRepository.CreateDoctorAsync(doctor, doctorDto.SpecializationIds);

                return doctor;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while registering the doctor.", ex);
            }
        }

        public async Task<bool> UpdateDoctorDetails(int doctorId, DoctorUpdateDto doctorDto)
        {
            try
            {
                var doctor = await _adminRepository.GetDoctorWithSpecializationsAsync(doctorId);

                if (doctor == null) return false;

                if (!string.IsNullOrEmpty(doctorDto.FullName)) doctor.FullName = doctorDto.FullName;
                if (!string.IsNullOrEmpty(doctorDto.Email)) doctor.Email = doctorDto.Email;
                if (doctorDto.ExperienceYears.HasValue) doctor.ExperienceYears = doctorDto.ExperienceYears.Value;
                if (!string.IsNullOrEmpty(doctorDto.Qualification)) doctor.Qualification = doctorDto.Qualification;
                if (!string.IsNullOrEmpty(doctorDto.Designation)) doctor.Designation = doctorDto.Designation;

                if (doctorDto.SpecializationIds != null && doctorDto.SpecializationIds.Any())
                {
                    await _adminRepository.UpdateDoctorSpecializationsAsync(doctorId, doctorDto.SpecializationIds);
                }

                await _adminRepository.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating doctor details.", ex);
            }
        }


        public async Task<bool> DeleteDoctor(int userId, int doctorId)
        {
            try
            {
                var doctor = await _adminRepository.GetDoctorByIdAndUserIdAsync(doctorId, userId);
                if (doctor == null) return false;

                doctor.UserID = null;
                doctor.Designation = "Inactive";

                var scheduledAppointments = await _adminRepository.GetScheduledAppointmentsAsync(doctorId);
                foreach (var appointment in scheduledAppointments)
                {
                    appointment.Status = "Canceled";
                }

                await _adminRepository.DeleteUserAsync(userId);
                await _adminRepository.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the doctor.", ex);
            }
        }

        public async Task<Doctor> GetDoctorDetails(int doctorId)
        {
            try
            {
                return await _adminRepository.GetDoctorByIdAsync(doctorId);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving doctor details.", ex);
            }
        }


        public async Task<Patient> UpdatePatient(PatientDto patientDto)
        {
            try
            {
                var patient = await _adminRepository.GetPatientByIdAsync(patientDto.PatientID);
                if (patient == null) throw new KeyNotFoundException($"Patient with ID {patientDto.PatientID} not found.");

                if (!string.IsNullOrEmpty(patientDto.FullName)) patient.FullName = patientDto.FullName;
                if (!string.IsNullOrEmpty(patientDto.Email)) patient.Email = patientDto.Email;
                patient.DateOfBirth = patientDto.DateOfBirth;
                if (!string.IsNullOrEmpty(patientDto.ContactNumber)) patient.ContactNumber = patientDto.ContactNumber;
                if (!string.IsNullOrEmpty(patientDto.Address)) patient.Address = patientDto.Address;
                if (!string.IsNullOrEmpty(patientDto.MedicalHistory)) patient.MedicalHistory = patientDto.MedicalHistory;

                await _adminRepository.SaveAsync();
                return patient;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating patient details.", ex);
            }
        }

        public async Task<Patient> GetPatientDetails(int patientId)
        {
            try
            {
                var patient = await _adminRepository.GetPatientByIdAsync(patientId);
                if (patient == null) throw new KeyNotFoundException($"Patient with ID {patientId} not found.");
                return patient;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving patient details.", ex);
            }
        }

        public async Task<bool> DeletePatient(int userId, int patientId)
        {
            try
            {
                var patient = await _adminRepository.GetPatientByIdAndUserIdAsync(patientId, userId);
                if (patient == null) return false;

                patient.UserID = null;

                var scheduledAppointments = await _adminRepository.GetScheduledAppointmentsByPatientIdAsync(patientId);
                foreach (var appointment in scheduledAppointments)
                {
                    appointment.Status = "Canceled";
                }

                await _adminRepository.DeleteUserAsync(userId);
                await _adminRepository.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the patient.", ex);
            }
        }


        public async Task<Appointment> RescheduleAppointment(int appointmentId, AppointmentRescheduleDto rescheduleDto)
        {
            try
            {
                var appointment = await _adminRepository.GetAppointmentWithDoctorByIdAsync(appointmentId);

                if (appointment == null)
                    throw new KeyNotFoundException("Appointment not found.");

                bool isOnSchedule = await _adminRepository.IsDoctorOnScheduleAsync(appointment.DoctorID, rescheduleDto.NewAppointmentDate);

                if (!isOnSchedule)
                    throw new InvalidOperationException("On new appointment date, doctor is not on schedule.");

                appointment.AppointmentDate = rescheduleDto.NewAppointmentDate;
                await _adminRepository.SaveAsync();

                return appointment;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while rescheduling the appointment.", ex);
            }
        }


        public async Task<Appointment> ViewAppointmentDetails(int appointmentId)
        {
            try
            {
                var appointment = await _adminRepository.GetAppointmentByIdAsync(appointmentId);

                if (appointment == null)
                    throw new KeyNotFoundException("Appointment not found.");

                return appointment;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving appointment details.", ex);
            }
        }

        public async Task<IEnumerable<ScheduleWithDoctorDto>> GetSchedulesWithDoctorNameAsync(int doctorId)
        {
            try
            {
                var schedules = await _adminRepository.GetSchedulesWithDoctorDetailsAsync(doctorId);

                if (schedules == null || !schedules.Any())
                    throw new KeyNotFoundException("No schedules found for this doctor.");

                return schedules.Select(schedule => new ScheduleWithDoctorDto
                {
                    ScheduleID = schedule.ScheduleID,
                    DoctorID = schedule.DoctorID,
                    DoctorName = schedule.Doctor.FullName,
                    StartDate = schedule.StartDate,
                    EndDate = schedule.EndDate,
                    Status = schedule.Status
                });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving schedules with doctor details.", ex);
            }
        }
        public async Task<bool> UpdateScheduleByAdminAsync(int scheduleId, UpdateScheduleDto updateDto)
        {
            try
            {
                var schedule = await _adminRepository.GetScheduleByIdAsync(scheduleId);

                if (schedule == null)
                    throw new KeyNotFoundException("Schedule not found.");

                schedule.StartDate = updateDto.StartDate;
                schedule.EndDate = updateDto.EndDate;
                schedule.Status = "Scheduled";

                await _adminRepository.UpdateScheduleAsync(schedule);
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while updating the schedule.", ex);
            }
        }


        public async Task<bool> CancelScheduleByAdminAsync(int scheduleId)
        {
            try
            {
                var schedule = await _adminRepository.GetScheduleByIdAsync(scheduleId);

                if (schedule == null || schedule.Status == "Completed")
                    throw new InvalidOperationException("Cannot cancel completed or non-existent schedule.");

                schedule.Status = "Cancelled";
                await _adminRepository.UpdateScheduleAsync(schedule);
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while canceling the schedule.", ex);
            }
        }

        public async Task<bool> MarkScheduleAsCompletedAsync(int scheduleId)
        {
            try
            {
                var schedule = await _adminRepository.GetScheduleByIdAsync(scheduleId);

                if (schedule == null || schedule.Status == "Cancelled")
                    throw new InvalidOperationException("Cannot mark cancelled or non-existent schedule as completed.");

                schedule.Status = "Completed";
                await _adminRepository.UpdateScheduleAsync(schedule);

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while marking the schedule as completed.", ex);
            }
        }



        // Method to update an existing schedule for a doctor with validation on DoctorID
        public async Task<bool> UpdateSchedule(int doctorId, int scheduleId, DateTime newStartDate, DateTime newEndDate)
        {
            try
            {
                var schedule = await _adminRepository.GetScheduleByIdAndDoctorIdAsync(scheduleId, doctorId);

                if (schedule == null)
                    throw new KeyNotFoundException($"Schedule with ID {scheduleId} not found for Doctor ID {doctorId}.");

                schedule.StartDate = newStartDate;
                schedule.EndDate = newEndDate;

                await _adminRepository.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while updating the schedule.", ex);
            }
        }

        public async Task<string> CancelSchedule(int doctorId, int scheduleId)
        {
            try
            {
                var schedule = await _adminRepository.GetScheduleByIdAndDoctorIdAsync(scheduleId, doctorId);

                if (schedule == null)
                    throw new KeyNotFoundException($"Schedule with ID {scheduleId} not found for Doctor ID {doctorId}.");

                if (schedule.Status == "Cancelled")
                    return "Schedule is already cancelled.";

                if (schedule.Status == "Completed")
                    return "Schedule is already completed and cannot be cancelled.";

                schedule.Status = "Cancelled";
                await _adminRepository.SaveAsync();
                return "Schedule cancelled successfully.";
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while canceling the schedule.", ex);
            }
        }

        public async Task<IEnumerable<BillingDetailsDto>> GetBillingDetailsWithNamesAsync()
        {
            try
            {
                var billings = await _adminRepository.GetBillingDetailsWithNamesAsync();

                if (billings == null || !billings.Any())
                    throw new KeyNotFoundException("No billing details found.");

                return billings.Select(b => new BillingDetailsDto
                {
                    BillingID = b.BillingID,
                    PatientID = b.PatientID,
                    PatientName = b.Patient.FullName,
                    DoctorID = b.DoctorID,
                    DoctorName = b.Doctor.FullName,
                    ConsultationFee = b.ConsultationFee,
                    TotalTestsPrice = b.TotalTestsPrice,
                    TotalMedicationsPrice = b.TotalMedicationsPrice,
                    GrandTotal = b.GrandTotal,
                    Status = b.Status
                });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving billing details.", ex);
            }
        }

        public async Task<bool> MarkBillAsPaidAsync(int billingId)
        {
            try
            {
                var billing = await _adminRepository.GetBillingByIdAsync(billingId);

                if (billing == null || billing.Status == "Paid")
                    throw new InvalidOperationException("Billing is either already paid or does not exist.");

                billing.Status = "Paid";
                await _adminRepository.UpdateBillingAsync(billing);

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while marking the bill as paid.", ex);
            }
        }

        //tests
        public async Task<IEnumerable<TestDto>> GetAllTestsAsync()
        {
            try
            {
                var tests = await _adminRepository.GetAllTestsAsync();

                if (tests == null || !tests.Any())
                    throw new KeyNotFoundException("No tests found.");

                return tests.Select(t => new TestDto
                {
                    TestID = t.TestID,
                    TestName = t.TestName,
                    TestPrice = t.TestPrice
                });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving tests.", ex);
            }
        }
        public async Task<bool> AddTestAsync(CreateUpdateTestDto createTestDto)
        {
            try
            {
                var test = new Test
                {
                    TestName = createTestDto.TestName,
                    TestPrice = createTestDto.TestPrice
                };

                await _adminRepository.AddTestAsync(test);
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while adding a new test.", ex);
            }
        }

        public async Task<bool> UpdateTestAsync(int testId, CreateUpdateTestDto updateTestDto)
        {
            try
            {
                var test = await _adminRepository.GetTestByIdAsync(testId);
                if (test == null)
                    throw new KeyNotFoundException("Test not found.");

                test.TestName = updateTestDto.TestName;
                test.TestPrice = updateTestDto.TestPrice;

                await _adminRepository.UpdateTestAsync(test);
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while updating the test.", ex);
            }
        }

        public async Task<IEnumerable<MedicationDto>> GetAllMedicationsAsync()
        {
            try
            {
                var medications = await _adminRepository.GetAllMedicationsAsync();

                if (medications == null || !medications.Any())
                    throw new KeyNotFoundException("No medications found.");

                return medications.Select(m => new MedicationDto
                {
                    MedicationID = m.MedicationID,
                    MedicationName = m.MedicationName,
                    PricePerUnit = m.PricePerUnit
                });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving medications.", ex);
            }
        }

        public async Task<bool> AddMedicationAsync(CreateUpdateMedicationDto createMedicationDto)
        {
            try
            {
                var medication = new Medication
                {
                    MedicationName = createMedicationDto.MedicationName,
                    PricePerUnit = createMedicationDto.PricePerUnit
                };

                await _adminRepository.AddMedicationAsync(medication);
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while adding a new medication.", ex);
            }
        }

        public async Task<bool> UpdateMedicationAsync(int medicationId, CreateUpdateMedicationDto updateMedicationDto)
        {
            try
            {
                var medication = await _adminRepository.GetMedicationByIdAsync(medicationId);
                if (medication == null)
                    throw new KeyNotFoundException("Medication not found.");

                medication.MedicationName = updateMedicationDto.MedicationName;
                medication.PricePerUnit = updateMedicationDto.PricePerUnit;

                await _adminRepository.UpdateMedicationAsync(medication);
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while updating the medication.", ex);
            }
        }



    }
}
