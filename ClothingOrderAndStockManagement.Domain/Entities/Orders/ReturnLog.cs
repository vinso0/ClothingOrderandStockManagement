namespace ClothingOrderAndStockManagement.Domain.Entities.Orders;

public partial class ReturnLog
{
    public int ReturnLogsId { get; set; }

    public int OrderRecordsId { get; set; }

    public int CustomerId { get; set; }

    public string UserId { get; set; } = null!;

    public DateOnly ReturnDate { get; set; }

    public string? Reason { get; set; }

    public string? ReturnStatus { get; set; }

    public virtual OrderRecord OrderRecords { get; set; } = null!;
}
