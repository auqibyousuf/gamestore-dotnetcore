using System.Security.Claims;
using GameStore.Backend.Data;
using GameStore.Backend.Dtos.Basket;
using GameStore.Backend.Dtos.Common;
using GameStore.Backend.Helpers;
using GameStore.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var userContext = UserContextHelper.GetUserContext(User);

            var basket = await _basketService.GetBasketAsync(userContext.UserId);

            return Ok(basket);
        }

        [HttpPost("items")]
        public async Task<IActionResult> UpsertItem([FromBody] UpsertBasketItemDto dto)
        {
            var userContext = UserContextHelper.GetUserContext(User);
            var updateItems = await _basketService.UpsertBasketItemAsync(userContext.UserId, dto.GameId, dto.Quantity);
            return Ok(updateItems);

        }

        [HttpDelete("items /{gameId}")]
        public async Task<IActionResult> DeleteItem(int gameId)
        {
            var userContext = UserContextHelper.GetUserContext(User);
            var updatedBasket = await _basketService.UpsertBasketItemAsync(userContext.UserId, gameId, 0);

            return Ok(BaseResponse<BasketResponseDto>.Ok(updatedBasket, "Item Removed"));
        }

        [HttpDelete]
        public async Task<IActionResult> ClearBasket()
        {
            var userContext = UserContextHelper.GetUserContext(User);
            var updatedBasket = await _basketService.ClearBasketAsync(userContext.UserId);

            return Ok(BaseResponse<BasketResponseDto>.Ok(updatedBasket, "Basket Cleared successfully"));
        }
    }
}
