using AmazeCareAPI.Data;
using AmazeCareAPI.Models;
using AmazeCareAPI.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmazeCareAPI.Repositories.Interface;

namespace AmazeCareAPI.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AmazeCareContext _context;

        public AdminRepository(AmazeCareContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return !await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Administrator> CreateAdminAsync(Administrator admin)
        {
            _context.Administrators.Add(admin);
            await _context.SaveChangesAsync();
            return admin;
        }

        public async Task<Doctor> CreateDoctorAsync(Doctor doctor, IEnumerable<int> specializationIds)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            if (specializationIds != null)
            {
                var doctorSpecializations = specializationIds.Select(id => new DoctorSpecialization
                {
                    DoctorID = doctor.DoctorID,
                    SpecializationID = id
                });
                _context.DoctorSpecializations.AddRange(doctorSpecializations);
                await _context.SaveChangesAsync();
            }

            return doctor;
        }

        public async Task<Doctor> GetDoctorByIdAsync(int doctorId)
        {
            return await _context.Doctors
                .Include(d => d.DoctorSpecializations)
                .ThenInclude(ds => ds.Specialization)
                .FirstOrDefaultAsync(d => d.DoctorID == doctorId);
        }

        public async Task<bool> UpdateDoctorAsync(Doctor doctor, IEnumerable<int> specializationIds)
        {
            var existingDoctor = await _context.Doctors
                .Include(d => d.DoctorSpecializations)
                .FirstOrDefaultAsync(d => d.DoctorID == doctor.DoctorID);

            if (existingDoctor == null) return false;

            // Update basic doctor details
            existingDoctor.FullName = doctor.FullName;
            existingDoctor.Email = doctor.Email;
            existingDoctor.ExperienceYears = doctor.ExperienceYears;
            existingDoctor.Qualification = doctor.Qualification;
            existingDoctor.Designation = doctor.Designation;

            // Update specializations
            var existingSpecializations = _context.DoctorSpecializations
                .Where(ds => ds.DoctorID == doctor.DoctorID);
            _context.DoctorSpecializations.RemoveRange(existingSpecializations);

            if (specializationIds != null)
            {
                var newSpecializations = specializationIds.Select(id => new DoctorSpecialization
                {
                    DoctorID = doctor.DoctorID,
                    SpecializationID = id
                });
                _context.DoctorSpecializations.AddRange(newSpecializations);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDoctorAsync(int userId, int doctorId)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorID == doctorId && d.UserID == userId);
            if (doctor == null) return false;

            doctor.UserID = null;
            doctor.Designation = "Inactive";
            _context.Doctors.Update(doctor);

            var appointments = await _context.Appointments
                .Where(a => a.DoctorID == doctorId && a.Status == "Scheduled")
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                appointment.Status = "Canceled";
            }

            _context.Appointments.UpdateRange(appointments);
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return true;
        }

       

        public async Task<bool> UpdatePatientAsync(Patient patient)
        {
            var existingPatient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientID == patient.PatientID);
            if (existingPatient == null) return false;

            existingPatient.FullName = patient.FullName;
            existingPatient.Email = patient.Email;
            existingPatient.DateOfBirth = patient.DateOfBirth;
            existingPatient.ContactNumber = patient.ContactNumber;
            existingPatient.Address = patient.Address;
            existingPatient.MedicalHistory = patient.MedicalHistory;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePatientAsync(int userId, int patientId)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientID == patientId && p.UserID == userId);
            if (patient == null) return false;

            patient.UserID = null;
            _context.Patients.Update(patient);

            var appointments = await _context.Appointments
                .Where(a => a.PatientID == patientId && a.Status == "Scheduled")
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                appointment.Status = "Canceled";
            }

            _context.Appointments.UpdateRange(appointments);

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Appointment?> RescheduleAppointmentAsync(Appointment appointment, DateTime newDate)
        {
            appointment.AppointmentDate = newDate;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

       

        public async Task<bool> UpdateDoctorScheduleAsync(int doctorId, int scheduleId, DateTime newStartDate, DateTime newEndDate)
        {
            var schedule = await _context.DoctorSchedule
                .FirstOrDefaultAsync(h => h.ScheduleID == scheduleId && h.DoctorID == doctorId);

            if (schedule == null) return false;

            schedule.StartDate = newStartDate;
            schedule.EndDate = newEndDate;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelDoctorScheduleAsync(int doctorId, int scheduleId)
        {
            var schedule = await _context.DoctorSchedule
                .FirstOrDefaultAsync(h => h.ScheduleID == scheduleId && h.DoctorID == doctorId);

            if (schedule == null) return false;

            schedule.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Doctor> GetDoctorWithSpecializationsAsync(int doctorId)
        {
            return await _context.Doctors
                .Include(d => d.DoctorSpecializations)
                .FirstOrDefaultAsync(d => d.DoctorID == doctorId);
        }

        public async Task UpdateDoctorSpecializationsAsync(int doctorId, IEnumerable<int> specializationIds)
        {
            var existingSpecializations = _context.DoctorSpecializations.Where(ds => ds.DoctorID == doctorId);
            _context.DoctorSpecializations.RemoveRange(existingSpecializations);

            foreach (var specializationId in specializationIds)
            {
                _context.DoctorSpecializations.Add(new DoctorSpecialization
                {
                    DoctorID = doctorId,
                    SpecializationID = specializationId
                });
            }
        }

        public async Task<Doctor> GetDoctorByIdAndUserIdAsync(int doctorId, int userId)
        {
            return await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorID == doctorId && d.UserID == userId);
        }

        public async Task<List<Appointment>> GetScheduledAppointmentsAsync(int doctorId)
        {
            return await _context.Appointments
                .Where(a => a.DoctorID == doctorId && a.Status == "Scheduled")
                .ToListAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
        }

        public async Task<Patient> GetPatientByIdAsync(int patientId)
        {
            return await _context.Patients.FirstOrDefaultAsync(p => p.PatientID == patientId);
        }
        public async Task<Patient> GetPatientByIdAndUserIdAsync(int patientId, int userId)
        {
            return await _context.Patients.FirstOrDefaultAsync(p => p.PatientID == patientId && p.UserID == userId);
        }

        public async Task<List<Appointment>> GetScheduledAppointmentsByPatientIdAsync(int patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientID == patientId && a.Status == "Scheduled")
                .ToListAsync();
        }
        public async Task<Appointment> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentID == appointmentId);
        }

        public async Task<Appointment> GetAppointmentWithDoctorByIdAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentID == appointmentId);
        }

        public async Task<bool> IsDoctorOnScheduleAsync(int doctorId, DateTime appointmentDate)
        {
            return await _context.DoctorSchedule
                .AnyAsync(h => h.DoctorID == doctorId && h.Status == "Scheduled" && appointmentDate >= h.StartDate && appointmentDate <= h.EndDate);
        }
        public async Task<DoctorSchedule> GetScheduleByIdAndDoctorIdAsync(int scheduleId, int doctorId)
        {
            return await _context.DoctorSchedule
                .FirstOrDefaultAsync(h => h.ScheduleID == scheduleId && h.DoctorID == doctorId);
        }

        public async Task<IEnumerable<Specialization>> GetAllSpecializationsAsync()
        {
            return await _context.Specializations
                .Include(s => s.DoctorSpecializations) // Include related DoctorSpecializations if needed
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorSchedule>> GetSchedulesWithDoctorDetailsAsync(int doctorId)
        {
            return await _context.DoctorSchedule
                .Include(ds => ds.Doctor) // Include doctor details
                .Where(ds => ds.DoctorID == doctorId)
                .ToListAsync();
        }

        public async Task<DoctorSchedule?> GetScheduleByIdAsync(int scheduleId)
        {
            return await _context.DoctorSchedule
                .Include(ds => ds.Doctor) // Include doctor details
                .FirstOrDefaultAsync(ds => ds.ScheduleID == scheduleId);
        }

        public async Task UpdateScheduleAsync(DoctorSchedule schedule)
        {
            _context.DoctorSchedule.Update(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Billing>> GetBillingDetailsWithNamesAsync()
        {
            return await _context.Billing
                .Include(b => b.Doctor)
                .Include(b => b.Patient)
                .ToListAsync();
        }

        public async Task<Billing?> GetBillingByIdAsync(int billingId)
        {
            return await _context.Billing
                .Include(b => b.Doctor)
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(b => b.BillingID == billingId);
        }

        public async Task UpdateBillingAsync(Billing billing)
        {
            _context.Billing.Update(billing);
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<Test>> GetAllTestsAsync()
        {
            return await _context.Tests.ToListAsync();
        }
        public async Task AddTestAsync(Test test)
        {
            await _context.Tests.AddAsync(test);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateTestAsync(Test test)
        {
            _context.Tests.Update(test);
            await _context.SaveChangesAsync();
        }
        public async Task<Test?> GetTestByIdAsync(int testId)
        {
            return await _context.Tests.FirstOrDefaultAsync(t => t.TestID == testId);
        }
        //medications 
        public async Task<IEnumerable<Medication>> GetAllMedicationsAsync()
        {
            return await _context.Medications.ToListAsync();
        }
        public async Task AddMedicationAsync(Medication medication)
        {
            await _context.Medications.AddAsync(medication);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateMedicationAsync(Medication medication)
        {
            _context.Medications.Update(medication);
            await _context.SaveChangesAsync();
        }
        public async Task<Medication?> GetMedicationByIdAsync(int medicationId)
        {
            return await _context.Medications.FirstOrDefaultAsync(m => m.MedicationID == medicationId);
        }


        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
