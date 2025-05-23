using System.Collections.Generic;

namespace CornerStore.DTOs;

public class OrderCreateDTO
{
    public int CashierId { get; set; }
    public List<OrderProductCreateDTO> Products { get; set; } = new();
}
