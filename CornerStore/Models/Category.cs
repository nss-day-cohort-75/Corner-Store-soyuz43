using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models;

public class Category
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = "";

    // Navigation
    public List<Product> Products { get; set; } = new();
}
