using System;

namespace GameStore.Backend.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;
    public string GameName { get; set; } = null!;

    public int GameID { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }
    public decimal SubTotal { get; set; }
}
