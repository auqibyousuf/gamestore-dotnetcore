namespace GameStore.Backend.Dtos.Basket;

public class BasketResponseDto
{
    public decimal Total { get; set; }
    public List<BasketItemDto> Items { get; set; } = [];
}
