namespace ClothingOrderAndStockManagement.Application.Dtos.Orders
{
    public class ReturnLogDto
    {
        public int ReturnLogsId { get; set; }
        public int OrderRecordsId { get; set; }
        public int OrderPackagesId { get; set; }
        public int CustomerId { get; set; }
        public DateOnly ReturnDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool RestockItems { get; set; } = true;

        // Navigation properties for display
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
        public DateOnly OrderDate { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
    }

    public class ReturnRequestDto
    {
        public int OrderRecordsId { get; set; }
        public int OrderPackagesId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool RestockItems { get; set; } = true;
    }

    public class CompletedOrderDto
    {
        public int OrderRecordsId { get; set; }
        public int OrderPackagesId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateOnly OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }
}
