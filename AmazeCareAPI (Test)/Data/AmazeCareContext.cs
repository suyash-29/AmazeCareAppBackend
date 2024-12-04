using System.Collections.Generic;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using AmazeCareAPI.Models;

namespace AmazeCareAPI.Data
{
    public class AmazeCareContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<DoctorSchedule> DoctorSchedule { get; set; }

        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<DoctorSpecialization> DoctorSpecializations { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }

        public DbSet<MedicalRecordTest> MedicalRecordTests { get; set; }
        public DbSet<PrescriptionMedication> PrescriptionMedications { get; set; }

        public DbSet<Test> Tests { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Billing> Billing { get; set; }

        public AmazeCareContext(DbContextOptions<AmazeCareContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define composite key for DoctorSpecialization
            modelBuilder.Entity<DoctorSpecialization>()
                .HasKey(ds => new { ds.DoctorID, ds.SpecializationID });

            // Additional configurations can be added here if needed
        }
    }

}

