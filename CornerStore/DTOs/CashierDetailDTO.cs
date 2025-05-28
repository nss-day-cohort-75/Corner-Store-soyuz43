namespace CornerStore.DTOs;

public class CashierDetailDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName => $"{FirstName} {LastName}";

    public List<OrderDTO> Orders { get; set; } = new();
}
