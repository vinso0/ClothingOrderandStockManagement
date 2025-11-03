using System;
using System.Collections.Generic;

namespace ClothingOrderAndStockManagement.Application.Dtos.Packages
{
    public class UpdatePackageItemDto
    {
        public int ItemId { get; set; }
        public int ItemQuantity { get; set; }
    }
}
