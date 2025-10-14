namespace ClothingOrderAndStockManagement.Domain.Entities.Orders;

public partial class PaymentRecord
{
    public int PaymentRecordsId { get; set; }

    public int OrderRecordsId { get; set; }

    public int CustomerId { get; set; }

    public decimal Amount { get; set; }

    public string? ProofUrl { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public virtual OrderRecord OrderRecords { get; set; } = null!;
}
