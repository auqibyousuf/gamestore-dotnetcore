using System;
using GameStore.Backend.Enums;

namespace GameStore.Backend.Models;

public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Provider { get; set; } = null!;

    public string Status { get; set; } = PaymentStatus.Pending.ToString();

    public string? ProviderPaymentId { get; set; }
    public string? FailureReason { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
