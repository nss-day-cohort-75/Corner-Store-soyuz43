using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CornerStore.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ProductName { get; set; } = "";

    [Required]
    public decimal Price { get; set; }

    [Required]
    public string Brand { get; set; } = "";

    [Required]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public List<OrderProduct> OrderProducts { get; set; } = new();
}
