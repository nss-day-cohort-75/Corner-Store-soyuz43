using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CornerStore.Models;

public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CashierId { get; set; }

    [Required]
    public Cashier Cashier { get; set; }

    public DateTime? PaidOnDate { get; set; }

    // Navigation
    public List<OrderProduct> OrderProducts { get; set; } = new();

    [NotMapped]
    public decimal Total => OrderProducts.Sum(op => op.Product.Price * op.Quantity);
}
