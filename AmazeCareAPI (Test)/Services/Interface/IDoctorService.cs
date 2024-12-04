using AmazeCareAPI.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmazeCareAPI.Services.Interface
{
    public interface IDoctorService
    {
        Task<int?> GetDoctorIdAsync(int userId);
        Task<List<AppointmentWithPatientDto>> GetAppointmentsByStatus(int doctorId, string status);
        Task<bool> ApproveAppointmentRequest(int doctorId, int appointmentId);
        Task<bool> CancelScheduledAppointment(int doctorId, int appointmentId);
        Task<bool> UpdateMedicalRecord(int doctorId, int recordId, int patientId, UpdateMedicalRecordDto updateDto);
        Task<List<PatientMedicalRecordDto>> GetMedicalRecordsByPatientIdAsync(int patientId);
        Task<bool> ConductConsultation(int doctorId, int appointmentId, CreateMedicalRecordDto recordDto, decimal consultationFee);
        Task<bool> UpdateBillingStatusAsync(int billingId, int doctorId);
        Task<List<TestDto>> GetTestsAsync();
        Task<List<MedicationDto>> GetMedicationsAsync();
        Task<bool> AddScheduleAsync(int doctorId, CreateScheduleDto scheduleDto);
        Task<bool> UpdateScheduleAsync(int scheduleId, int doctorId, UpdateScheduleDto updateDto);
        Task<IEnumerable<ScheduleDto>> GetSchedulesAsync(int doctorId);
        Task<bool> CancelScheduleAsync(int scheduleId, int doctorId);
        Task<bool> CompleteScheduleAsync(int scheduleId, int doctorId);
        Task<(bool Success, string Message)> RescheduleAppointmentAsync(int doctorId, int appointmentId, AppointmentRescheduleDto rescheduleDto);

        Task<IEnumerable<DoctorBillingDetailsDto>> GetBillsByDoctorIdAsync(int doctorId);
    }

}
