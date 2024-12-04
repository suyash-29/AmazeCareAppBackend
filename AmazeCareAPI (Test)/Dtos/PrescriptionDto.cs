namespace AmazeCareAPI.Dtos
{
    public class PrescriptionDto
    {
        public int MedicationID { get; set; }
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public int DurationDays { get; set; }
        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }
    }

}
