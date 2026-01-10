using System;
using GameStore.Backend.Dtos.Payment;
using GameStore.Backend.Enums;
using GameStore.Backend.Settings;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Razorpay.Api;

namespace GameStore.Backend.Services.Payments;

public class RazorPaymentProvider(IOptions<RazorSettings> options, ILogger<RazorPaymentProvider> logger) : IPaymentProvider
{
    private readonly RazorSettings _settings = options.Value;
    private readonly ILogger<RazorPaymentProvider> _logger = logger;



    public async Task<PaymentProviderResultDto> CreatePaymentAsync(int orderId, int userId, decimal amount)
    {


        var client = new RazorpayClient(_settings.KeyId, _settings.KeySecret);
        long amountInPaise = (long)(amount * 100);
        var options = new Dictionary<string, object>
        {
            { "amount", amountInPaise },
            { "currency", "INR" },
            { "receipt", $"order_{orderId}" }
        };

        var razorpayOrder = client.Order.Create(options);
        string razorpayOrderId = razorpayOrder["id"].ToString();

        _logger.LogInformation(
            "Razorpay order created. OrderId={OrderId}, RazorpayOrderId={RazorpayOrderId}",
            orderId,
            razorpayOrderId
            );

        return new PaymentProviderResultDto
        {
            Success = true,
            Provider = "Razorpay",
            Status = PaymentStatus.Pending.ToString(),
            ProviderPaymentId = razorpayOrderId,
            Message = "Razorpay order created successfully"

        };

    }


    public async Task<PaymentProviderResultDto> ConfirmPaymentAsync(string providerPaymentId)
    {
        var client = new RazorpayClient(_settings.KeyId, _settings.KeySecret);

        var payment = client.Payment.Fetch(providerPaymentId);

        var status = payment["status"].ToString();

        if (status == "captured" || status == "authorized")
        {
            return new PaymentProviderResultDto
            {
                Success = true,
                ProviderPaymentId = payment["id"],
                Message = "Payment confirmed Successfully"
            };
        }

        else
        {
            return new PaymentProviderResultDto
            {
                Success = false,
                ProviderPaymentId = providerPaymentId,
                Message = $"Payment not successful. Status = {status}"
            };
        }
    }
    public async Task<PaymentProviderResultDto> FailPaymentAsync(string providerPaymentId, string reason)
    {
        var client = new RazorpayClient(_settings.KeyId, _settings.KeySecret);

        var payment = client.Payment.Fetch(providerPaymentId);

        var status = payment["status"].ToString();

        if (status == "captured" || status == "authorized")
        {
            throw new InvalidOperationException(
                "Cannot mark a successful Razorpay payment as failed"
            );
        }

        // failed / created / cancelled etc.
        return new PaymentProviderResultDto
        {
            Success = false,
            ProviderPaymentId = providerPaymentId,
            Message = string.IsNullOrWhiteSpace(reason)
                ? $"Payment failed with status {status}"
                : reason
        };
    }
}
