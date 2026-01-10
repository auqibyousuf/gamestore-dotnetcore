using System;

namespace GameStore.Backend.Dtos.Orders.Timeline;

public class AdminTimelineDto
{
    public DateTime Time { get; set; }
    public string EventType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public string PaymentId { get; set; } = null!;
    public string FailureReason { get; set; } = null!;
    public decimal Amount { get; set; }
}
