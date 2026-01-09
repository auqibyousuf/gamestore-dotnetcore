using System;
using GameStore.Backend.Enums;

namespace GameStore.Backend.Dtos.Admin;

public class AdminOrderFilterDto
{
    public OrderPaymentStatus? PaymentStatus { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
}
