using System.Security.Claims;
using GameStore.Backend.Dtos.Common;
using GameStore.Backend.Dtos.Orders;
using GameStore.Backend.Dtos.Orders.Timeline;
using GameStore.Backend.Helpers;
using GameStore.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Backend.Controllers
{
    [Authorize]
    [Route("api/orders")]
    [ApiController]
    public class OrdersController(OrderService orderService, ILogger<OrdersController> logger) : ControllerBase
    {

        private readonly OrderService _orderService = orderService;
        private readonly ILogger<OrdersController> _logger = logger;
        [HttpPost("/checkout")]
        public async Task<ActionResult> Checkout()
        {
            var userContext = UserContextHelper.GetUserContext(User);
            var order = await _orderService.CreateOrderAsync(userContext.UserId);

            return Ok(BaseResponse<OrderResponseDto>.Ok(order, "Order Created Successfully"));
        }


        [HttpGet("my")]
        public async Task<ActionResult> GetMyOrders()
        {
            var userContext = UserContextHelper.GetUserContext(User);

            var myOrder = await _orderService.GetMyOrdersAsync(userContext.UserId);
            return Ok(BaseResponse<List<OrderResponseDto>>.Ok(myOrder, "Order Created Successfully"));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetOrderById(int Id)
        {
            var userContext = UserContextHelper.GetUserContext(User);
            var orderById = await _orderService.GetOrderByIdAsync(userContext.UserId, Id, userContext.Role);
            return Ok(BaseResponse<OrderResponseDto>.Ok(orderById, "Order Fetched Successfully"));
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("{orderId}/mark-paid")]
        public async Task<ActionResult> MarkPaid(int orderId)
        {
            var orderStatus = await _orderService.MarkOrderAsPaidAsync(orderId);

            return Ok(BaseResponse<OrderResponseDto>.Ok(orderStatus, "Order marked as Paid"));
        }

        [HttpPost("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userContext = UserContextHelper.GetUserContext(User);

            var result = await _orderService.CancelOrderAsync(orderId, userContext.UserId, userContext.Role);

            return Ok(
                BaseResponse<OrderResponseDto>.Ok(
                    result,
                    "Order cancelled successfully"
                )
            );

        }

        [HttpPost("{orderId}/timeline")]
        public async Task<IActionResult> GetOrderTimeline(int orderId)
        {
            var userContext = UserContextHelper.GetUserContext(User);

            var timeline = await _orderService.GetOrderTimelineAsync(
                orderId,
                userContext.UserId,
                userContext.Role
            );

            return Ok(
                BaseResponse<List<TimelineDto>>.Ok(
                    timeline,
                    "Order timeline fetched successfully"
                )
            );
        }
    }
}
