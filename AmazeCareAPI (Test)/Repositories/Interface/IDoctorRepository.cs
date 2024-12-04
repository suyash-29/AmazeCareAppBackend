using AmazeCareAPI.Dtos;
using AmazeCareAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmazeCareAPI.Repositories.Interface
{
    public interface IDoctorRepository
    {
        Task<int?> GetDoctorIdByUserIdAsync(int userId);
        Task<List<AppointmentWithPatientDto>> GetAppointmentsByStatusAsync(int doctorId, string status);
        Task<Appointment> GetScheduledAppointmentAsync(int doctorId, int appointmentId);
        Task<Appointment?> GetRequestedAppointmentAsync(int doctorId, int appointmentId);
        Task<MedicalRecord> GetMedicalRecordAsync(int doctorId, int recordId, int patientId);
        Task UpdateMedicalRecordAsync(MedicalRecord medicalRecord);
        Task AddMedicalRecordAsync(MedicalRecord medicalRecord);
        Task<List<Test>> GetTestsByIdsAsync(IEnumerable<int> testIds);
        Task AddMedicalRecordTestsAsync(IEnumerable<MedicalRecordTest> medicalRecordTests);
        Task AddPrescriptionsAsync(IEnumerable<Prescription> prescriptions);
        Task UpdateMedicalRecordTotalPriceAsync(MedicalRecord medicalRecord);
        Task AddBillingAsync(Billing billing);
        Task UpdatePrescriptionBillingIdsAsync(IEnumerable<Prescription> prescriptions);

        Task<List<Appointment>> GetAppointmentsWithMedicalRecordsAndDetailsAsync(int patientId);

        Task<Medication> GetMedicationByIdAsync(int medicationId);

        Task<Billing> GetBillingByIdAndDoctorIdAsync(int billingId, int doctorId);
        Task UpdateBillingAsync(Billing billing);
        Task<List<TestDto>> GetAllTestsAsync();
        Task<List<MedicationDto>> GetAllMedicationsAsync();

        Task AddScheduleAsync(DoctorSchedule schedule);
        Task<DoctorSchedule> GetScheduleByIdAndDoctorIdAsync(int scheduleId, int doctorId);
        Task UpdateScheduleAsync(DoctorSchedule schedule);
        Task<IEnumerable<ScheduleDto>> GetSchedulesByDoctorIdAsync(int doctorId);
        Task<Appointment> GetAppointmentByIdAndDoctorIdAsync(int appointmentId, int doctorId);
        Task<bool> IsOnScheduleAsync(int doctorId, DateTime date);
        Task UpdateAppointmentAsync(Appointment appointment);

        Task<IEnumerable<Billing>> GetBillsByDoctorIdAsync(int doctorId);
    }
}
