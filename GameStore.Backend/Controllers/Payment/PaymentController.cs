using GameStore.Backend.Dtos.Common;
using GameStore.Backend.Dtos.Payment;
using GameStore.Backend.Helpers;
using GameStore.Backend.Services.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Backend.Controllers.Payment
{
    [Authorize]
    [Route("api/payments")]
    [ApiController]
    public class PaymentController(PaymentService paymentService) : ControllerBase
    {
        private readonly PaymentService _paymentService = paymentService;

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
    }
}
