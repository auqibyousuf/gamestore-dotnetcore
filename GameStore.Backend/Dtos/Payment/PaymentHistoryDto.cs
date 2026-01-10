using System;

namespace GameStore.Backend.Dtos.Payment;

public class PaymentHistoryDto
{
    public int OrderId { get; set; }
    public List<PaymentHistoryItemDto> Payments { get; set; } = [];
}
