namespace AmazeCareAPI.Dtos
{
    public class CreatePrescriptionDto
    {
        public int MedicationID { get; set; }
        public string Dosage { get; set; }
        public int DurationDays { get; set; }
        public int Quantity { get; set; }
    }
}
