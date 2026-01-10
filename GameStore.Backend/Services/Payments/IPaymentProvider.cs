using GameStore.Backend.Dtos.Payment;

namespace GameStore.Backend.Services.Payments;

public interface IPaymentProvider
{


    Task<PaymentProviderResultDto> CreatePaymentAsync(
          int orderId,
          int userId,
          decimal amount
      );

    Task<PaymentProviderResultDto> ConfirmPaymentAsync(
        string providerPaymentId
    );

    Task<PaymentProviderResultDto> FailPaymentAsync(
        string providerPaymentId,
        string reason
    );
}
