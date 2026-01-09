using System;

namespace GameStore.Backend.Dtos.Admin;

public class AdminOrderItemDto
{
    public int GameId { get; set; }
    public string GameName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}
