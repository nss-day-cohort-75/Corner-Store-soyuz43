using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CornerStore.Models;

public class OrderProduct
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    public Order Order { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public Product Product { get; set; }

    [Required]
    public int Quantity { get; set; }
}
