namespace GameStore.Backend.Dtos.Orders;

public class OrderResponseDto
{
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public List<OrderItemResponseDto> Items { get; set; } = [];
}
