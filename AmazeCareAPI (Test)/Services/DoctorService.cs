using AmazeCareAPI.Data;
using AmazeCareAPI.Dtos;
using AmazeCareAPI.Models;
using AmazeCareAPI.Repositories.Interface;
using AmazeCareAPI.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace AmazeCareAPI.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;

        public DoctorService(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<int?> GetDoctorIdAsync(int userId)
        {
            try
            {
                return await _doctorRepository.GetDoctorIdByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error fetching doctor ID for user {userId}.", ex);
            }
        }


            public async Task<List<AppointmentWithPatientDto>> GetAppointmentsByStatus(int doctorId, string status)
            {
                try
                {
                    return await _doctorRepository.GetAppointmentsByStatusAsync(doctorId, status);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Error fetching appointments with status '{status}' for doctor {doctorId}.", ex);
                }
            }

            public async Task<bool> ApproveAppointmentRequest(int doctorId, int appointmentId)
            {
                try
                {
                    var appointment = await _doctorRepository.GetRequestedAppointmentAsync(doctorId, appointmentId);
                    if (appointment == null)
                        return false;

                    appointment.Status = "Scheduled";
                    await _doctorRepository.UpdateAppointmentAsync(appointment);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Error approving appointment {appointmentId} for doctor {doctorId}.", ex);
                }
            }

            public async Task<bool> CancelScheduledAppointment(int doctorId, int appointmentId)
            {
                try
                {
                    var appointment = await _doctorRepository.GetScheduledAppointmentAsync(doctorId, appointmentId);
                    if (appointment == null)
                        return false;

                    appointment.Status = "Canceled";
                    await _doctorRepository.UpdateAppointmentAsync(appointment);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Error canceling appointment {appointmentId} for doctor {doctorId}.", ex);
                }
            }
        public async Task<bool> UpdateMedicalRecord(int doctorId, int recordId, int patientId, UpdateMedicalRecordDto updateDto)
        {
            try
            {
                var medicalRecord = await _doctorRepository.GetMedicalRecordAsync(doctorId, recordId, patientId);
                if (medicalRecord == null)
                    return false;

                if (updateDto.Symptoms != null)
                    medicalRecord.Symptoms = updateDto.Symptoms;
                if (updateDto.PhysicalExamination != null)
                    medicalRecord.PhysicalExamination = updateDto.PhysicalExamination;
                if (updateDto.TreatmentPlan != null)
                    medicalRecord.TreatmentPlan = updateDto.TreatmentPlan;
                if (updateDto.FollowUpDate != null)
                    medicalRecord.FollowUpDate = updateDto.FollowUpDate;

                await _doctorRepository.UpdateMedicalRecordAsync(medicalRecord);
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating medical record {recordId} for doctor {doctorId}.", ex);
            }
        }
        public async Task<bool> ConductConsultation(int doctorId, int appointmentId, CreateMedicalRecordDto recordDto, decimal consultationFee)
        {
            try
            {
                var appointment = await _doctorRepository.GetScheduledAppointmentAsync(doctorId, appointmentId);
                if (appointment == null || appointment.Status != "Scheduled")
                    return false;

                var medicalRecord = new MedicalRecord
                {
                    AppointmentID = appointment.AppointmentID,
                    DoctorID = doctorId,
                    PatientID = appointment.PatientID,
                    Symptoms = recordDto.Symptoms,
                    PhysicalExamination = recordDto.PhysicalExamination,
                    TreatmentPlan = recordDto.TreatmentPlan,
                    FollowUpDate = recordDto.FollowUpDate,
                    TotalPrice = 0
                };

                await _doctorRepository.AddMedicalRecordAsync(medicalRecord);

                decimal totalTestsPrice = 0;
                decimal totalMedicationsPrice = 0;

                if (recordDto.TestIDs != null && recordDto.TestIDs.Any())
                {
                    var selectedTests = await _doctorRepository.GetTestsByIdsAsync(recordDto.TestIDs);
                    var medicalRecordTests = selectedTests.Select(test => new MedicalRecordTest
                    {
                        RecordID = medicalRecord.RecordID,
                        TestID = test.TestID
                    }).ToList();

                    await _doctorRepository.AddMedicalRecordTestsAsync(medicalRecordTests);
                    totalTestsPrice = selectedTests.Sum(t => t.TestPrice);
                    medicalRecord.TotalPrice += totalTestsPrice;
                }

                var prescriptions = new List<Prescription>();
                if (recordDto.Prescriptions != null && recordDto.Prescriptions.Any())
                {
                    foreach (var prescriptionDto in recordDto.Prescriptions)
                    {
                        var medication = await _doctorRepository.GetMedicationByIdAsync(prescriptionDto.MedicationID);
                        if (medication != null)
                        {
                            var prescription = new Prescription
                            {
                                RecordID = medicalRecord.RecordID,
                                MedicationID = prescriptionDto.MedicationID,
                                Dosage = prescriptionDto.Dosage,
                                DurationDays = prescriptionDto.DurationDays,
                                Quantity = prescriptionDto.Quantity,
                                TotalPrice = medication.PricePerUnit * prescriptionDto.Quantity,
                                MedicationName = medication.MedicationName
                            };
                            prescriptions.Add(prescription);
                            totalMedicationsPrice += prescription.TotalPrice;
                        }
                    }
                    await _doctorRepository.AddPrescriptionsAsync(prescriptions);
                    medicalRecord.TotalPrice += totalMedicationsPrice;
                }

                await _doctorRepository.UpdateMedicalRecordTotalPriceAsync(medicalRecord);

                var billing = new Billing
                {
                    PatientID = appointment.PatientID,
                    DoctorID = doctorId,
                    MedicalRecordID = medicalRecord.RecordID,
                    ConsultationFee = consultationFee,
                    TotalTestsPrice = totalTestsPrice,
                    TotalMedicationsPrice = totalMedicationsPrice,
                    GrandTotal = consultationFee + totalTestsPrice + totalMedicationsPrice,
                    Status = "Pending"
                };
                await _doctorRepository.AddBillingAsync(billing);

                medicalRecord.BillingID = billing.BillingID;
                await _doctorRepository.UpdateBillingAsync(billing);

                foreach (var prescription in prescriptions)
                {
                    prescription.BillingID = billing.BillingID;
                }

                await _doctorRepository.UpdatePrescriptionBillingIdsAsync(prescriptions);

                appointment.Status = "Completed";
                await _doctorRepository.UpdateAppointmentAsync(appointment);

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error conducting consultation for appointment {appointmentId} with doctor {doctorId}.", ex);
            }
        }

        public async Task<List<PatientMedicalRecordDto>> GetMedicalRecordsByPatientIdAsync(int patientId)
        {
            try
            {
                var records = await _doctorRepository.GetAppointmentsWithMedicalRecordsAndDetailsAsync(patientId);
                if (records == null || !records.Any())
                    throw new Exception("No medical records found for the given patient ID.");

                var result = records.Select(a => new PatientMedicalRecordDto
                {
                    MedicalRecordID = a.MedicalRecord?.RecordID,
                    AppointmentDate = a.AppointmentDate,
                    DoctorName = a.Doctor?.FullName,
                    Symptoms = a.MedicalRecord?.Symptoms,
                    PhysicalExamination = a.MedicalRecord?.PhysicalExamination,
                    TreatmentPlan = a.MedicalRecord?.TreatmentPlan,
                    FollowUpDate = a.MedicalRecord?.FollowUpDate,
                    Tests = a.MedicalRecord?.MedicalRecordTests?.Select(mt => new TestDto
                    {
                        TestName = mt.Test?.TestName,
                        TestPrice = mt.Test.TestPrice
                    }).ToList() ?? new List<TestDto>(),
                    Prescriptions = a.MedicalRecord?.Prescriptions?.Select(p => new PrescriptionDto
                    {
                        MedicationName = p.MedicationName,
                        Dosage = p.Dosage,
                        DurationDays = p.DurationDays,
                        Quantity = p.Quantity
                    }).ToList() ?? new List<PrescriptionDto>(),
                    BillingDetails = a.MedicalRecord?.Billing != null ? new BillingDto
                    {
                        BillingID = a.MedicalRecord.Billing.BillingID,
                        ConsultationFee = a.MedicalRecord.Billing.ConsultationFee,
                        TotalTestsPrice = a.MedicalRecord.Billing.TotalTestsPrice,
                        TotalMedicationsPrice = a.MedicalRecord.Billing.TotalMedicationsPrice,
                        GrandTotal = a.MedicalRecord.Billing.GrandTotal,
                        Status = a.MedicalRecord.Billing.Status
                    } : null
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching medical records: {ex.Message}", ex);
            }
        }


        public async Task<bool> UpdateBillingStatusAsync(int billingId, int doctorId)
        {
            try
            {
                var billing = await _doctorRepository.GetBillingByIdAndDoctorIdAsync(billingId, doctorId);
                if (billing == null)
                    throw new Exception("Billing record not found.");

                if (billing.Status == "Paid")
                    throw new Exception("Billing record is already marked as paid.");

                billing.Status = "Paid";
                await _doctorRepository.UpdateBillingAsync(billing);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating billing status: {ex.Message}", ex);
            }
        }

        public async Task<List<TestDto>> GetTestsAsync()
        {
            try
            {
                var tests = await _doctorRepository.GetAllTestsAsync();
                if (tests == null || !tests.Any())
                    throw new Exception("No tests available.");

                return tests;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching tests: {ex.Message}", ex);
            }
        }
        public async Task<List<MedicationDto>> GetMedicationsAsync()
        {
            try
            {
                var medications = await _doctorRepository.GetAllMedicationsAsync();
                if (medications == null || !medications.Any())
                    throw new Exception("No medications available.");

                return medications;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching medications: {ex.Message}", ex);
            }
        }
        public async Task<bool> AddScheduleAsync(int doctorId, CreateScheduleDto scheduleDto)
        {
            try
            {
                var schedule = new DoctorSchedule
                {
                    DoctorID = doctorId,
                    StartDate = scheduleDto.StartDate,
                    EndDate = scheduleDto.EndDate,
                    Status = "Scheduled"
                };

                await _doctorRepository.AddScheduleAsync(schedule);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while adding the schedule: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateScheduleAsync(int scheduleId, int doctorId, UpdateScheduleDto updateDto)
        {
            try
            {
                var schedule = await _doctorRepository.GetScheduleByIdAndDoctorIdAsync(scheduleId, doctorId);
                if (schedule == null)
                    throw new Exception("Schedule not found.");

                schedule.StartDate = updateDto.StartDate;
                schedule.EndDate = updateDto.EndDate;
                schedule.Status = updateDto.Status;

                await _doctorRepository.UpdateScheduleAsync(schedule);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the schedule: {ex.Message}", ex);
            }
        }
        public async Task<IEnumerable<ScheduleDto>> GetSchedulesAsync(int doctorId)
        {
            try
            {
                return await _doctorRepository.GetSchedulesByDoctorIdAsync(doctorId);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching schedules: {ex.Message}", ex);
            }
        }
        public async Task<bool> CancelScheduleAsync(int scheduleId, int doctorId)
        {
            try
            {
                var schedule = await _doctorRepository.GetScheduleByIdAndDoctorIdAsync(scheduleId, doctorId);
                if (schedule == null)
                    throw new Exception("Schedule not found.");

                if (schedule.Status == "Completed")
                    throw new Exception("Cannot cancel a completed schedule.");

                schedule.Status = "Cancelled";
                await _doctorRepository.UpdateScheduleAsync(schedule);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while canceling the schedule: {ex.Message}", ex);
            }
        }
        public async Task<bool> CompleteScheduleAsync(int scheduleId, int doctorId)
        {
            try
            {
                var schedule = await _doctorRepository.GetScheduleByIdAndDoctorIdAsync(scheduleId, doctorId);
                if (schedule == null)
                    throw new Exception("Schedule not found.");

                if (schedule.Status == "Cancelled")
                    throw new Exception("Cannot complete a cancelled schedule.");

                schedule.Status = "Completed";
                await _doctorRepository.UpdateScheduleAsync(schedule);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while completing the schedule: {ex.Message}", ex);
            }
        }



        public async Task<(bool Success, string Message)> RescheduleAppointmentAsync(int doctorId, int appointmentId, AppointmentRescheduleDto rescheduleDto)
        {
            try
            {
                var appointment = await _doctorRepository.GetAppointmentByIdAndDoctorIdAsync(appointmentId, doctorId);
                if (appointment == null)
                    return (false, "Appointment not found or unauthorized access.");

                if (rescheduleDto.NewAppointmentDate <= DateTime.Now)
                    return (false, "The new appointment date and time must be in the future.");

                bool isOnSchedule = await _doctorRepository.IsOnScheduleAsync(doctorId, rescheduleDto.NewAppointmentDate);
                if (!isOnSchedule)
                    return (false, "The new appointment date conflicts with the doctor's schedule.");

                appointment.AppointmentDate = rescheduleDto.NewAppointmentDate;
                await _doctorRepository.UpdateAppointmentAsync(appointment);
                return (true, "Appointment rescheduled successfully.");
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while rescheduling the appointment: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<DoctorBillingDetailsDto>> GetBillsByDoctorIdAsync(int doctorId)
        {
            try
            {
                var bills = await _doctorRepository.GetBillsByDoctorIdAsync(doctorId);
                if (bills == null || !bills.Any())
                    throw new Exception("No bills found for the doctor.");

                return bills.Select(b => new DoctorBillingDetailsDto
                {
                    BillingID = b.BillingID,
                    DoctorID = b.DoctorID,
                    DoctorName = b.Doctor.FullName,
                    PatientID = b.PatientID,
                    PatientName = b.Patient.FullName,
                    ConsultationFee = b.ConsultationFee,
                    TotalTestsPrice = b.TotalTestsPrice,
                    TotalMedicationsPrice = b.TotalMedicationsPrice,
                    GrandTotal = b.GrandTotal,
                    Status = b.Status
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching bills: {ex.Message}", ex);
            }
        }


    }
}
