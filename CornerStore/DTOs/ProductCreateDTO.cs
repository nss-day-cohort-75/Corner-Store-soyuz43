namespace CornerStore.DTOs;

public class ProductCreateDTO
{
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
    public string Brand { get; set; } = "";
    public int CategoryId { get; set; }
}
