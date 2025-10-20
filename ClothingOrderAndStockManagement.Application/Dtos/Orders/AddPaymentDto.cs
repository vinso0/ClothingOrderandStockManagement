using Microsoft.AspNetCore.Http;

namespace ClothingOrderAndStockManagement.Application.Dtos.Orders
{
    public class AddPaymentDto
    {
        public int OrderRecordsId { get; set; }
        public decimal Amount { get; set; }
        public string? ProofUrl { get; set; }
        public string? ProofUrl2 { get; set; }
        public string PaymentStatus { get; set; } = "Down Payment";
    }
}