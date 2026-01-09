using System;
using GameStore.Backend.Enums;

namespace GameStore.Backend.Dtos.Payment;

public class PaymentResultDto
{
    public string PaymentId { get; set; } = null!;
    public string Status { get; set; } = PaymentStatus.Pending.ToString();
    public string Provider { get; set; } = null!;
    public string? RedirectUrl { get; set; }
    public string? Message { get; set; }
    public decimal Amount { get; set; }
    public int OrderId { get; set; }
}
