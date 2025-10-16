namespace ClothingOrderAndStockManagement.Application.Dtos.Orders
{
    public class PaymentRecordDto
    {
        public int PaymentRecordsId { get; set; }
        public decimal Amount { get; set; }
        public string? ProofUrl { get; set; }
        public string PaymentStatus { get; set; } = null!;
    }
}
