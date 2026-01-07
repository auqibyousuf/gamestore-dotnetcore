using System.Security.Claims;
using GameStore.Backend.Data;
using GameStore.Backend.Dtos.Basket;
using GameStore.Backend.Dtos.Common;
using GameStore.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Backend.Controllers
{
    [ApiController]
    [Route("api/basket")]
    [Authorize]
    public class BasketController(BasketService basketService, ILogger<BasketController> logger, GameStoreContext context) : ControllerBase
    {
        private readonly BasketService _basketService = basketService;
        private readonly ILogger<BasketController> _logger = logger;
        private readonly GameStoreContext _context = context;


        [HttpGet]
        public async Task<IActionResult> GetByBasket()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var basket = await _basketService.GetBasketAsync(userId);

            return Ok(basket);
        }

        [HttpPost("items")]
        public async Task<IActionResult> UpsertItem([FromBody] UpsertBasketItemDto dto)
        {
            var getUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (getUserId is null)
                return Unauthorized();

            var userId = int.Parse(getUserId);
            var updateItems = await _basketService.UpsertBasketItemAsync(userId, dto.GameId, dto.Quantity);
            return Ok(updateItems);

        }

        [HttpDelete("items /{gameId}")]
        public async Task<IActionResult> DeleteItem(int gameId)
        {
            var getUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (getUserId is null)
                return Unauthorized();

            var userId = int.Parse(getUserId);
            var updatedBasket = await _basketService.UpsertBasketItemAsync(userId, gameId, 0);

            return Ok(BaseResponse<BasketResponseDto>.Ok(updatedBasket, "Item Removed"));
        }

        [HttpDelete]
        public async Task<IActionResult> ClearBasket()
        {
            var getUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (getUserId is null)
                return Unauthorized();

            var userId = int.Parse(getUserId);
            var updatedBasket = await _basketService.ClearBasketAsync(userId);

            return Ok(BaseResponse<BasketResponseDto>.Ok(updatedBasket, "Basket Cleared successfully"));
        }
    }
}
