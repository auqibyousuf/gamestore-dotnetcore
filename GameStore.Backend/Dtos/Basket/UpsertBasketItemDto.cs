using System;

namespace GameStore.Backend.Dtos.Basket;

public class UpsertBasketItemDto
{

    public int GameId { get; set; }
    public int Quantity { get; set; }
}
