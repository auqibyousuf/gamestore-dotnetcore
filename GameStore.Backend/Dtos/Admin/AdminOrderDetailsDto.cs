using System;

namespace GameStore.Backend.Dtos.Admin;

public class AdminOrderDetailsDto
{
    // Order info
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = null!;
    public string? PaymentMethod { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    // User info
    public int UserId { get; set; }
    public string UserEmail { get; set; } = null!;

    // Order items
    public List<AdminOrderItemDto> Items { get; set; } = [];
}
