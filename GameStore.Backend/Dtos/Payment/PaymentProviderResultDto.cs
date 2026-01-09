namespace GameStore.Backend.Dtos.Payment
{
    public class PaymentProviderResultDto
    {
        public string ProviderPaymentId { get; set; } = null!;
        public string Provider { get; set; } = null!;
        public bool Success { get; set; }
        public string? Message { get; set; }

        public string Status { get; set; } = null!;
        public decimal Amount { get; set; }
    }
}
