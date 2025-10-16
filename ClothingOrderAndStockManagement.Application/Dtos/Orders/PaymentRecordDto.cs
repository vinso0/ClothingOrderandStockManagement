namespace ClothingOrderAndStockManagement.Application.Dtos.Orders
{
    public class PaymentRecordDto
    {
        public int PaymentRecordsId { get; set; }
        public decimal Amount { get; set; }
        public string? ProofUrl { get; set; }
        public string? ProofUrl2 { get; set; } // Second proof
        public string PaymentStatus { get; set; } = null!;
        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }
}
