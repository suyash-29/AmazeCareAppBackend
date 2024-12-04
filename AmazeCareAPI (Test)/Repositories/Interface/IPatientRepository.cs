using AmazeCareAPI.Models;
using AmazeCareAPI.Dtos;

namespace AmazeCareAPI.Repositories.Interface
{
    public interface IPatientRepository
    {
        Task<Patient?> GetPatientByUserIdAsync(int userId);
        Task UpdatePatientAsync(Patient patient);
        Task<IEnumerable<DoctorDto>> SearchDoctorsAsync(string? specialization);
        Task AddAppointmentAsync(Appointment appointment);
        Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);
        Task UpdateAppointmentAsync(Appointment appointment);
        Task<IEnumerable<AppointmentWithDoctorDto>> GetAppointmentsByPatientIdAsync(int patientId);
        Task<List<PatientMedicalRecordDto>> GetMedicalHistoryAsync(int patientId);
        Task<bool> IsDoctorOnSchedule(int doctorId, DateTime appointmentDate);
        Task<Appointment?> GetAppointmentByIdAsync(int patientId, int appointmentId);
        Task<int> SaveChangesAsync();
        Task<int> GetPatientIdByUserIdAsync(int userId);
        Task<List<PatientTestDetailDto>> GetTestDetailsByPatientIdAsync(int patientId);
        Task<List<PatientPrescriptionDetailDto>> GetPrescriptionDetailsByPatientIdAsync(int patientId);
        Task<List<BillingDto>> GetBillingDetailsByPatientIdAsync(int patientId);
        Task<Appointment?> GetAppointmentByIdAndPatientIdAsync(int appointmentId, int patientId);
        Task<bool> IsDoctorOnScheduleAsync(int doctorId, DateTime newAppointmentDate);


        Task<User?> GetUserByIdAsync(int userId);
        Task UpdateUserAsync(User user);
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<List<DoctorSchedule>> GetDoctorScheduleAsync(int doctorId);

        Task<IEnumerable<Billing>> GetBillsByPatientIdAsync(int patientId);
    }
}
