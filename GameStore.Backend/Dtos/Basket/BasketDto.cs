using System;

namespace GameStore.Backend.Dtos.Basket;

public class BasketDto
{
    public List<BasketItemDto> Items { get; set; } = [];
    public decimal Total {get; set;}
}
