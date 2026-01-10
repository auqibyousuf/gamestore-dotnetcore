using System;

namespace GameStore.Backend.Dtos.Payment;

public class PaymentHistoryItemDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Provider { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<PaymentHistoryItemDto> Payments { get; set; } = [];
}
