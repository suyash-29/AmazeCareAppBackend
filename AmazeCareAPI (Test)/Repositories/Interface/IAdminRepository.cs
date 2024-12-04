using AmazeCareAPI.Models;
using AmazeCareAPI.Dtos;

namespace AmazeCareAPI.Repositories.Interface
{
    public interface IAdminRepository
    {
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<User> CreateUserAsync(User user);
        Task<Administrator> CreateAdminAsync(Administrator admin);
        Task<Doctor> CreateDoctorAsync(Doctor doctor, IEnumerable<int> specializationIds);
        Task<Doctor> GetDoctorByIdAsync(int doctorId);
        Task<bool> UpdateDoctorAsync(Doctor doctor, IEnumerable<int> specializationIds);
        Task<bool> DeleteDoctorAsync(int userId, int doctorId);
        Task<Patient> GetPatientByIdAsync(int patientId);
        Task<bool> UpdatePatientAsync(Patient patient);
        Task<bool> DeletePatientAsync(int userId, int patientId);
        Task<Appointment?> RescheduleAppointmentAsync(Appointment appointment, DateTime newDate);
        Task<Appointment> GetAppointmentByIdAsync(int appointmentId);
        Task<bool> UpdateDoctorScheduleAsync(int doctorId, int scheduleId, DateTime newStartDate, DateTime newEndDate);
        Task<bool> CancelDoctorScheduleAsync(int doctorId, int scheduleId);


        Task<Doctor> GetDoctorWithSpecializationsAsync(int doctorId);
        Task UpdateDoctorSpecializationsAsync(int doctorId, IEnumerable<int> specializationIds);
        Task<Doctor> GetDoctorByIdAndUserIdAsync(int doctorId, int userId);
        Task<List<Appointment>> GetScheduledAppointmentsAsync(int doctorId);
        Task DeleteUserAsync(int userId);
        Task<Patient> GetPatientByIdAndUserIdAsync(int patientId, int userId);
        Task<List<Appointment>> GetScheduledAppointmentsByPatientIdAsync(int patientId);

        Task<DoctorSchedule> GetScheduleByIdAndDoctorIdAsync(int scheduleId, int doctorId);
        Task<bool> IsDoctorOnScheduleAsync(int doctorId, DateTime appointmentDate);
        Task<Appointment> GetAppointmentWithDoctorByIdAsync(int appointmentId);

        Task<IEnumerable<Specialization>> GetAllSpecializationsAsync();
        Task<IEnumerable<DoctorSchedule>> GetSchedulesWithDoctorDetailsAsync(int doctorId);
        Task<DoctorSchedule?> GetScheduleByIdAsync(int scheduleId);

        Task UpdateScheduleAsync(DoctorSchedule schedule);

        Task<IEnumerable<Billing>> GetBillingDetailsWithNamesAsync();

        Task<Billing?> GetBillingByIdAsync(int billingId);

        Task UpdateBillingAsync(Billing billing);
        Task<IEnumerable<Test>> GetAllTestsAsync();
        Task AddTestAsync(Test test);
        Task UpdateTestAsync(Test test);

        Task<Test?> GetTestByIdAsync(int testId);
        Task<IEnumerable<Medication>> GetAllMedicationsAsync();
        Task AddMedicationAsync(Medication medication);
        Task UpdateMedicationAsync(Medication medication);
        Task<Medication?> GetMedicationByIdAsync(int medicationId);



        Task SaveAsync();
    }
}
