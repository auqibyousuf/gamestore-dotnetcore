using System.Security.Claims;
using GameStore.Backend.Dtos.Admin;
using GameStore.Backend.Dtos.Common;
using GameStore.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Backend.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin")]
    [ApiController]
    public class AdminOrdersController(OrderService orderService, ILogger<AdminOrdersController> logger) : ControllerBase
    {
        private readonly OrderService _orderService = orderService;
        private readonly ILogger<AdminOrdersController> _logger = logger;


        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] AdminOrderFilterDto filter)
        {
            var orders = await _orderService.GetAllOrdersAdminAsync(filter);

            return Ok(BaseResponse<List<AdminOrderListDto>>.Ok(orders, "Orders Fetched"));
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var order = await _orderService.GetOrderDetailsForAdminAsync(orderId);

            return Ok(
                BaseResponse<AdminOrderDetailsDto>
                    .Ok(order, "Order details fetched")
            );
        }
    }
}
