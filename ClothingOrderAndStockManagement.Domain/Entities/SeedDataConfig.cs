using System.Collections.Generic;

namespace ClothingOrderAndStockManagement.Domain.Entities
{
    public class SeedDataConfig
    {
        public List<string> Roles { get; set; } = new();
        public List<SeedUser> Users { get; set; } = new();
    }

    public class SeedUser
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}