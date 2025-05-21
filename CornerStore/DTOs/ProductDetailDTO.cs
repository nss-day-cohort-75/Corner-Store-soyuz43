namespace CornerStore.DTOs;

public class ProductDetailDTO
{
    public int Id { get; set; }
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
    public string Brand { get; set; } = "";
    public string CategoryName { get; set; } = "";
}
