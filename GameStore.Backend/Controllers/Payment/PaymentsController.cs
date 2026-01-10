using GameStore.Backend.Dtos.Common;
using GameStore.Backend.Dtos.Payment;
using GameStore.Backend.Helpers;
using GameStore.Backend.Services.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Backend.Controllers.Payment
{
    [Authorize]
    [Route("api/payments")]
    [ApiController]
    public class PaymentsController(PaymentService paymentService, ILogger<PaymentsController> logger) : ControllerBase
    {
        private readonly PaymentService _paymentService = paymentService;
        private readonly ILogger<PaymentsController> _logger = logger;

        [HttpPost("start/{orderId}")]
        public async Task<IActionResult> StartPayment(int orderId)
        {
            var userContext = UserContextHelper.GetUserContext(User);
            var payment = await _paymentService.StartPaymentAsync(orderId, userContext.UserId);

            return Ok(BaseResponse<PaymentResultDto>.Ok(payment, "Starting Patyment"));
        }

        [HttpPost("confirm/{providerPaymentId}")]
        public async Task<IActionResult> ConfirmPayment(string providerPaymentId)
        {
            var confirmPayment = await _paymentService.ConfirmPaymentAsync(providerPaymentId);
            return Ok(BaseResponse<PaymentResultDto>.Ok(confirmPayment, "Payment Successfull"));
        }

        [HttpPost("fail/{providerPaymentId}")]
        public async Task<IActionResult> FailPayment(string providerPaymentId, [FromRoute] string reason)
        {
            var failPayment = await _paymentService.FailPaymentAsync(providerPaymentId, reason);
            return Ok(BaseResponse<PaymentResultDto>.Ok(failPayment, "Payment Failed"));
        }

        [HttpPost("{orderId}/retry")]
        public async Task<IActionResult> RetryPayment(int orderId)
        {
            var userContext = UserContextHelper.GetUserContext(User);
            var retryPayment = await _paymentService.RetryPaymentAsync(orderId, userContext.UserId);
            return Ok(BaseResponse<RetryPaymentDto>.Ok(retryPayment, "Payment Successfull"));
        }

        [HttpPost("history")]
        public async Task<IActionResult> GetMyPayments()
        {
            var userContext = UserContextHelper.GetUserContext(User);
            _logger.LogInformation(
                "Fetching payment history for UserId={UserId}",
                userContext.UserId
            );
            var payments = await _paymentService.GetMyPaymentsAsync(userContext.UserId);
            return Ok(
                BaseResponse<List<PaymentHistoryItemDto>>
                    .Ok(payments, "Payment history fetched successfully")
            );
        }
        [HttpGet("orders/{orderId}")]
        public async Task<IActionResult> GetOrderPayments(int orderId)
        {
            var userContext = UserContextHelper.GetUserContext(User);

            _logger.LogInformation(
                "Fetching payment history for OrderId={OrderId}, UserId={UserId}",
                orderId,
                userContext.UserId
            );

            var history = await _paymentService
                .GetOrderPaymentsAsync(userContext.UserId, orderId);

            return Ok(
                BaseResponse<PaymentHistoryDto>
                    .Ok(history, "Order payment history fetched successfully")
            );
        }

    }
}
