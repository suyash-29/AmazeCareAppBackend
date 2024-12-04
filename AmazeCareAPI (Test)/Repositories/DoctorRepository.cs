using AmazeCareAPI.Data;
using AmazeCareAPI.Dtos;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazeCareAPI.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly AmazeCareContext _context;

        public DoctorRepository(AmazeCareContext context)
        {
            _context = context;
        }
        public async Task<int?> GetDoctorIdByUserIdAsync(int userId)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == userId);
            return doctor?.DoctorID;
        }
        public async Task<List<AppointmentWithPatientDto>> GetAppointmentsByStatusAsync(int doctorId, string status)
        {
            return await _context.Appointments
                .Where(a => a.DoctorID == doctorId && a.Status == status)
                .Include(a => a.Patient)
                .Select(a => new AppointmentWithPatientDto
                {
                    AppointmentID = a.AppointmentID,
                    PatientID = a.PatientID,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    Symptoms = a.Symptoms,
                    PatientName = a.Patient.FullName,
                    MedicalHistory = a.Patient.MedicalHistory
                })
                .ToListAsync();
        }
        public async Task<Appointment?> GetRequestedAppointmentAsync(int doctorId, int appointmentId)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.AppointmentID == appointmentId &&
                    a.DoctorID == doctorId &&
                    a.Status == "Requested");
        }

        public async Task<Appointment> GetScheduledAppointmentAsync(int doctorId, int appointmentId)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentID == appointmentId && a.DoctorID == doctorId && a.Status == "Scheduled");
        }

        public async Task<MedicalRecord> GetMedicalRecordAsync(int doctorId, int recordId, int patientId)
        {
            return await _context.MedicalRecords
                .FirstOrDefaultAsync(r => r.RecordID == recordId && r.DoctorID == doctorId && r.PatientID == patientId);
        }

        public async Task UpdateMedicalRecordAsync(MedicalRecord medicalRecord)
        {
            _context.MedicalRecords.Update(medicalRecord);
            await _context.SaveChangesAsync();
        }

        public async Task AddMedicalRecordAsync(MedicalRecord medicalRecord)
        {
            _context.MedicalRecords.Add(medicalRecord);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Test>> GetTestsByIdsAsync(IEnumerable<int> testIds)
        {
            return await _context.Tests.Where(t => testIds.Contains(t.TestID)).ToListAsync();
        }

        public async Task AddMedicalRecordTestsAsync(IEnumerable<MedicalRecordTest> medicalRecordTests)
        {
            _context.MedicalRecordTests.AddRange(medicalRecordTests);
            await _context.SaveChangesAsync();
        }

        public async Task AddPrescriptionsAsync(IEnumerable<Prescription> prescriptions)
        {
            _context.Prescriptions.AddRange(prescriptions);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMedicalRecordTotalPriceAsync(MedicalRecord medicalRecord)
        {
            _context.MedicalRecords.Update(medicalRecord);
            await _context.SaveChangesAsync();
        }

        public async Task AddBillingAsync(Billing billing)
        {
            _context.Billing.Add(billing);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePrescriptionBillingIdsAsync(IEnumerable<Prescription> prescriptions)
        {
            foreach (var prescription in prescriptions)
            {
                _context.Prescriptions.Update(prescription);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsWithMedicalRecordsAndDetailsAsync(int patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientID == patientId && a.MedicalRecord != null)
                .Include(a => a.Doctor)
                .Include(a => a.MedicalRecord)
                    .ThenInclude(m => m.MedicalRecordTests)
                        .ThenInclude(mt => mt.Test)
                .Include(a => a.MedicalRecord)
                    .ThenInclude(m => m.Prescriptions)
                .Include(a => a.MedicalRecord)
                    .ThenInclude(m => m.Billing)
                .ToListAsync();
        }

        public async Task<Medication> GetMedicationByIdAsync(int medicationId)
        {
            return await _context.Medications.FindAsync(medicationId);
        }

        public async Task<Billing> GetBillingByIdAndDoctorIdAsync(int billingId, int doctorId)
        {
            return await _context.Billing
                .FirstOrDefaultAsync(b => b.BillingID == billingId && b.DoctorID == doctorId);
        }

        public async Task UpdateBillingAsync(Billing billing)
        {
            _context.Billing.Update(billing);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TestDto>> GetAllTestsAsync()
        {
            return await _context.Tests
                .Select(t => new TestDto
                {
                    TestID = t.TestID,
                    TestName = t.TestName,
                    TestPrice = t.TestPrice
                })
                .ToListAsync();
        }

        public async Task<List<MedicationDto>> GetAllMedicationsAsync()
        {
            return await _context.Medications
                .Select(m => new MedicationDto
                {
                    MedicationID = m.MedicationID,
                    MedicationName = m.MedicationName,
                    PricePerUnit = m.PricePerUnit
                })
                .ToListAsync();
        }

        public async Task AddScheduleAsync(DoctorSchedule schedule)
        {
            _context.DoctorSchedule.Add(schedule);
            await _context.SaveChangesAsync();
        }
        public async Task<DoctorSchedule> GetScheduleByIdAndDoctorIdAsync(int scheduleId, int doctorId)
        {
            return await _context.DoctorSchedule
                .FirstOrDefaultAsync(h => h.ScheduleID == scheduleId && h.DoctorID == doctorId);
        }

        public async Task UpdateScheduleAsync(DoctorSchedule schedule)
        {
            _context.DoctorSchedule.Update(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ScheduleDto>> GetSchedulesByDoctorIdAsync(int doctorId)
        {
            return await _context.DoctorSchedule
                .Where(h => h.DoctorID == doctorId)
                .Select(h => new ScheduleDto
                {
                    ScheduleID = h.ScheduleID,
                    StartDate = h.StartDate,
                    EndDate = h.EndDate,
                    Status = h.Status
                })
                .ToListAsync();
        }
        public async Task<Appointment> GetAppointmentByIdAndDoctorIdAsync(int appointmentId, int doctorId)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentID == appointmentId && a.DoctorID == doctorId);
        }
        public async Task<bool> IsOnScheduleAsync(int doctorId, DateTime date)
        {
            return await _context.DoctorSchedule
                .AnyAsync(h => h.DoctorID == doctorId
                               && h.Status == "Scheduled"
                               && date >= h.StartDate
                               && date <= h.EndDate);
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Billing>> GetBillsByDoctorIdAsync(int doctorId)
        {
            return await _context.Billing
                .Include(b => b.Doctor) // Include Doctor for DoctorName
                .Include(b => b.Patient) // Include Patient for PatientName
                .Where(b => b.DoctorID == doctorId)
                .ToListAsync();
        }



    }
}
