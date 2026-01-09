using GameStore.Backend.Data;
using GameStore.Backend.Dtos.Payment;
using GameStore.Backend.Enums;
using GameStore.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Backend.Services.Payments;

public class ManualPaymentProvider : IPaymentProvider
{
    public async Task<PaymentProviderResultDto> ConfirmPaymentAsync(string providerPaymentId)
    {
        return await Task.FromResult(new PaymentProviderResultDto
        {
            Success = true,
            ProviderPaymentId = providerPaymentId,
            Status = PaymentStatus.Paid.ToString(),
            Message = "Payment confirmed",
            Provider = "Manual"
        });
    }

    public async Task<PaymentProviderResultDto> CreatePaymentAsync(int orderId, int userId, decimal amount)
    {
        var providerPaymentId = Guid.NewGuid().ToString();

        return await Task.FromResult(new PaymentProviderResultDto
        {
            Success = true,
            ProviderPaymentId = providerPaymentId,
            Provider = "Manual",
            Status = PaymentStatus.Pending.ToString(),
            Amount = amount,
            Message = "Manual payment created"
        });
    }

    public async Task<PaymentProviderResultDto> FailPaymentAsync(string providerPaymentId, string reason)
    {
        return await Task.FromResult(new PaymentProviderResultDto
        {
            Success = false,
            ProviderPaymentId = providerPaymentId,
            Status = PaymentStatus.Failed.ToString(),
            Message = reason
        });
    }
}
