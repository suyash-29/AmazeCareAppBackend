using AmazeCareAPI.Data;
using AmazeCareAPI.Models;
using AmazeCareAPI.Dtos;
using Microsoft.EntityFrameworkCore;
using AmazeCareAPI.Repositories.Interface;

namespace AmazeCareAPI.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly AmazeCareContext _context;

        public PatientRepository(AmazeCareContext context)
        {
            _context = context;
        }
        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return !await _context.Users.AnyAsync(u => u.Username == username);
        }


        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetPatientIdByUserIdAsync(int userId)
        {
            return await _context.Patients
                .Where(p => p.UserID == userId)
                .Select(p => p.PatientID)
                .FirstOrDefaultAsync();
        }
        public async Task<Patient?> GetPatientByUserIdAsync(int userId)
        {
            return await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userId);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task UpdatePatientAsync(Patient patient)
        {
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsDoctorOnSchedule(int doctorId, DateTime appointmentDate)
        {
            return await _context.DoctorSchedule.AnyAsync(h =>
                h.DoctorID == doctorId && h.Status == "Scheduled" &&
                appointmentDate >= h.StartDate && appointmentDate <= h.EndDate);
        }

        public async Task<List<DoctorSchedule>> GetDoctorScheduleAsync(int doctorId)
        {
            return await _context.DoctorSchedule
                .Where(h => h.DoctorID == doctorId && h.Status == "Scheduled")
                .ToListAsync();
        }

        public async Task AddAppointmentAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
        }

        public async Task<IEnumerable<DoctorDto>> SearchDoctorsAsync(string? specialization)
        {
            var query = _context.Doctors.AsQueryable();
            // only active doctors 
            query = query.Where(d => d.Designation != "Inactive");

            if (!string.IsNullOrEmpty(specialization))
            {
                query = query
                    .Where(d => d.DoctorSpecializations
                        .Any(ds => ds.Specialization.SpecializationName == specialization));
            }

            return await query.Select(d => new DoctorDto
            {
                DoctorID = d.DoctorID,
                FullName = d.FullName,
                ExperienceYears = d.ExperienceYears,
                Qualification = d.Qualification,
                Designation = d.Designation,
                Email = d.Email,
                Specializations = d.DoctorSpecializations
                    .Select(ds => ds.Specialization.SpecializationName)
                    .ToList()
                
            }).ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _context.Appointments.FindAsync(appointmentId);
        }
        public async Task<Appointment?> GetAppointmentByIdAsync(int patientId, int appointmentId)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(a => a.PatientID == patientId && a.AppointmentID == appointmentId);
        }
        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }

        

        public async Task<IEnumerable<AppointmentWithDoctorDto>> GetAppointmentsByPatientIdAsync(int patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientID == patientId)
                .Include(a => a.Doctor) // Include the related Doctor entity
                .Select(a => new AppointmentWithDoctorDto
                {
                    AppointmentID = a.AppointmentID,
                    PatientID = a.PatientID,
                    DoctorID = a.DoctorID,
                    DoctorName = a.Doctor.FullName, // Assuming Doctor has a FullName property
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                })
                .ToListAsync();
        }

        // Medical History
        public async Task<List<PatientMedicalRecordDto>> GetMedicalHistoryAsync(int patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientID == patientId && a.MedicalRecord != null)
                .Include(a => a.Doctor)
                .Include(a => a.MedicalRecord)
                    .ThenInclude(m => m.MedicalRecordTests)
                        .ThenInclude(mt => mt.Test)
                .Include(a => a.MedicalRecord)
                    .ThenInclude(m => m.Prescriptions)
                .Select(a => new PatientMedicalRecordDto
                {
                    MedicalRecordID = a.MedicalRecord.RecordID,
                    AppointmentDate = a.AppointmentDate,
                    DoctorName = a.Doctor.FullName,
                    Symptoms = a.MedicalRecord.Symptoms,
                    PhysicalExamination = a.MedicalRecord.PhysicalExamination,
                    TreatmentPlan = a.MedicalRecord.TreatmentPlan,
                    FollowUpDate = a.MedicalRecord.FollowUpDate,
                    Tests = a.MedicalRecord.MedicalRecordTests.Select(mt => new TestDto
                    {
                        TestName = mt.Test.TestName,
                        TestPrice = mt.Test.TestPrice
                    }).ToList(),
                    Prescriptions = a.MedicalRecord.Prescriptions.Select(p => new PrescriptionDto
                    {
                        MedicationName = p.MedicationName,
                        Dosage = p.Dosage,
                        DurationDays = p.DurationDays,
                        Quantity = p.Quantity
                    }).ToList(),
                    BillingDetails = _context.Billing
                        .Where(b => b.PatientID == patientId && b.MedicalRecordID == a.MedicalRecord.RecordID)
                        .Select(b => new BillingDto
                        {
                            BillingID = b.BillingID,
                            ConsultationFee = b.ConsultationFee,
                            TotalTestsPrice = b.TotalTestsPrice,
                            TotalMedicationsPrice = b.TotalMedicationsPrice,
                            GrandTotal = b.GrandTotal,
                            Status = b.Status
                        }).FirstOrDefault()
                }).ToListAsync();
        }

        
        public async Task<List<PatientTestDetailDto>> GetTestDetailsByPatientIdAsync(int patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientID == patientId)
                .Include(a => a.Doctor)
                .Include(a => a.MedicalRecord)
                    .ThenInclude(m => m.MedicalRecordTests)
                        .ThenInclude(mt => mt.Test)
                .SelectMany(a => a.MedicalRecord.MedicalRecordTests.Select(mt => new PatientTestDetailDto
                {
                    AppointmentId = a.AppointmentID,
                    DoctorName = a.Doctor.FullName,
                    TestId = mt.Test.TestID,
                    TestName = mt.Test.TestName,
                    TestPrice = mt.Test.TestPrice
                }))
                .ToListAsync();
        }

        public async Task<List<PatientPrescriptionDetailDto>> GetPrescriptionDetailsByPatientIdAsync(int patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientID == patientId)
                .Include(a => a.Doctor)
                .Include(a => a.MedicalRecord)
                    .ThenInclude(m => m.Prescriptions)
                .SelectMany(a => a.MedicalRecord.Prescriptions.Select(p => new PatientPrescriptionDetailDto
                {
                    AppointmentId = a.AppointmentID,
                    DoctorName = a.Doctor.FullName ?? "N/A",          
                    MedicationName = p.MedicationName ?? string.Empty,   
                    Dosage = p.Dosage ?? string.Empty,                  
                    DurationDays = p.DurationDays, 
                    Quantity = p.Quantity
                }))
                .ToListAsync();
        }

        public async Task<List<BillingDto>> GetBillingDetailsByPatientIdAsync(int patientId)
        {
            return await _context.Billing
                .Where(b => b.PatientID == patientId)
                .Select(b => new BillingDto
                {
                    BillingID = b.BillingID,
                    ConsultationFee = b.ConsultationFee,
                    TotalTestsPrice = b.TotalTestsPrice,
                    TotalMedicationsPrice = b.TotalMedicationsPrice,
                    GrandTotal = b.GrandTotal,
                    Status = b.Status
                })
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAndPatientIdAsync(int appointmentId, int patientId)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentID == appointmentId && a.PatientID == patientId);
        }

        public async Task<bool> IsDoctorOnScheduleAsync(int doctorId, DateTime newAppointmentDate)
        {
            return await _context.DoctorSchedule
                .AnyAsync(h => h.DoctorID == doctorId
                               && h.Status == "Scheduled"
                               && newAppointmentDate >= h.StartDate
                               && newAppointmentDate <= h.EndDate);
        }

        public async Task<IEnumerable<Billing>> GetBillsByPatientIdAsync(int patientId)
        {
            return await _context.Billing
                .Include(b => b.Doctor) // Include Doctor for DoctorName
                .Include(b => b.Patient) // Include Patient for PatientName
                .Where(b => b.PatientID == patientId)
                .ToListAsync();
        }



    }
}
