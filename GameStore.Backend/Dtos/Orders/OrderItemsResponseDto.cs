using GameStore.Backend.Models;

namespace GameStore.Backend.Dtos.Orders
{
    public class OrderItemResponseDto
    {
        public int GameId { get; set; }
        public string GameName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }
}
