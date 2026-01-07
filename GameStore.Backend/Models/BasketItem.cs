using System;

namespace GameStore.Backend.Models;

public class BasketItem
{

    public int Id { get; set; }
    public Basket Basket { get; set; } = null!;
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

}
