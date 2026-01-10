namespace GameStore.Backend.Dtos.Orders.Timeline;

public class TimelineDto
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Message { get; set; } = null!;
}
