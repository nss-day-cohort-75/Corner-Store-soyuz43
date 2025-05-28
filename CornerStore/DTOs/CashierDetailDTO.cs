namespace CornerStore.DTOs;

public class CashierDetailDTO
{
    public CashierDTO Cashier { get; set; } = new();
    public List<OrderDTO> Orders { get; set; } = new();
}
