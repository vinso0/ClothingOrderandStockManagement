
namespace ClothingOrderAndStockManagement.Application.Dtos.Users
{
    public class EditUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }
}
