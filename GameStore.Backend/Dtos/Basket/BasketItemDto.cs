using System;

namespace GameStore.Backend.Dtos.Basket;

public class BasketItemDto
{

    public int GameId { get; set; }
    public string GameName { get; set; } = null!;
    public int UnitPrice { get; set; }
    public int LinePrice { get; set; }

}
