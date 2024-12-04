using AmazeCareAPI.Dtos;

namespace AmazeCareAPI.Dtos
{
    public class CreateMedicalRecordDto
    {
        public string Symptoms { get; set; }
        public string PhysicalExamination { get; set; }
        public string TreatmentPlan { get; set; }
        public DateTime? FollowUpDate { get; set; }


        public List<int> TestIDs { get; set; } = new List<int>();

        public List<CreatePrescriptionDto> Prescriptions { get; set; } = new List<CreatePrescriptionDto>();
    }
}