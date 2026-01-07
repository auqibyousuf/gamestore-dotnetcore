using System;

namespace GameStore.Backend.Dtos.Basket;

public class BasketItemDto
{

    public int GameId { get; set; }
    public string GameName { get; set; } = null!;
    public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }

}
