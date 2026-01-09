using System;
using GameStore.Backend.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Backend.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = OrderPaymentStatus.Pending.ToString();
    public string? PaymentMethod { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public List<OrderItem> Items { get; set; } = [];
}
