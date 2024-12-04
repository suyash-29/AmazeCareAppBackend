namespace AmazeCareAPI.Dtos
{
    public class BillingDto
    {
        public int BillingID { get; set; }
        public decimal ConsultationFee { get; set; }
        public decimal TotalTestsPrice { get; set; }
        public decimal TotalMedicationsPrice { get; set; }
        public decimal GrandTotal { get; set; }
        public string Status { get; set; }
    }
}
