using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClothingOrderAndStockManagement.Application.Dtos.Packages
{
    public class UpdatePackageDto
    {
        public int PackagesId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public List<UpdatePackageItemDto> PackageItems { get; set; } = new();
    }

    public class UpdatePackageItemDto
    {
        public int ItemId { get; set; }
        public int ItemQuantity { get; set; }
    }
}
