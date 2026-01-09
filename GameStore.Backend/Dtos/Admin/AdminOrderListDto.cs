namespace GameStore.Backend.Dtos.Admin;

public class AdminOrderListDto
{
    public int OrderId { get; set; }

    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public string PaymentStatus { get; set; } = null!;
    public string? PaymentMethod { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

}
