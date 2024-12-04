using AmazeCareAPI.Models;
using AmazeCareAPI.Dtos;

namespace AmazeCareAPI.Services.Interface
{
    public interface IAdminService
    {
        Task<(bool IsAvailable, string Message)> CheckUsernameAvailabilityAsync(string username);
        Task<Administrator> RegisterAdmin(string username, string password, string fullName, string email);
        Task<Doctor> RegisterDoctor(DoctorRegistrationDto doctorDto);
        Task<bool> UpdateDoctorDetails(int doctorId, DoctorUpdateDto doctorDto);
        Task<bool> DeleteDoctor(int userId, int doctorId);
        Task<Patient> UpdatePatient(PatientDto patientDto);
        Task<Doctor> GetDoctorDetails(int doctorId);
        Task<Patient> GetPatientDetails(int patientId);
        Task<bool> DeletePatient(int userId, int patientId);
        Task<Appointment> RescheduleAppointment(int appointmentId, AppointmentRescheduleDto rescheduleDto);
        Task<Appointment> ViewAppointmentDetails(int appointmentId);
        Task<bool> UpdateSchedule(int doctorId, int scheduleId, DateTime newStartDate, DateTime newEndDate);
        Task<string> CancelSchedule(int doctorId, int scheduleId);
        Task<IEnumerable<SpecializationDto>> GetAllSpecializationsAsync();
        Task<IEnumerable<ScheduleWithDoctorDto>> GetSchedulesWithDoctorNameAsync(int doctorId);
        Task<bool> MarkScheduleAsCompletedAsync(int scheduleId);
        Task<bool> CancelScheduleByAdminAsync(int scheduleId);
        Task<bool> UpdateScheduleByAdminAsync(int scheduleId, UpdateScheduleDto updateDto);

        Task<IEnumerable<BillingDetailsDto>> GetBillingDetailsWithNamesAsync();

        Task<bool> MarkBillAsPaidAsync(int billingId);

        Task<bool> UpdateMedicationAsync(int medicationId, CreateUpdateMedicationDto updateMedicationDto);
        Task<bool> AddMedicationAsync(CreateUpdateMedicationDto createMedicationDto);
        Task<IEnumerable<MedicationDto>> GetAllMedicationsAsync();
        Task<bool> UpdateTestAsync(int testId, CreateUpdateTestDto updateTestDto);

        Task<bool> AddTestAsync(CreateUpdateTestDto createTestDto);
        Task<IEnumerable<TestDto>> GetAllTestsAsync();



    }
}
