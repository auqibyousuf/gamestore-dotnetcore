using GameStore.Backend.Data;
using GameStore.Backend.Dtos.Basket;
using GameStore.Backend.Dtos.Common;
using GameStore.Backend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Backend.Services;

public class BasketService(GameStoreContext context)
{

    private readonly GameStoreContext _context = context;

    public async Task<BasketResponseDto> UpsertBasketItemAsync(int userId, int gameId, int qty)
    {
        if (qty < 0)
        {
            throw new ArgumentException("Quantity cannot be less than 0");
        }
        var basket = await _context.Baskets.Include(basket => basket.Items).ThenInclude(g => g.Game).FirstOrDefaultAsync(user => user.UserId == userId);

        if (basket == null)
        {
            basket = new Basket
            {
                UserId = userId
            };
            _context.Baskets.Add(basket);
            await _context.SaveChangesAsync();
        }

        var existingItem = basket.Items.FirstOrDefault(i => i.GameId == gameId);

        if (existingItem != null)
        {
            if (qty == 0)
            {
                _context.BasketItems.Remove(existingItem);
            }
            else
            {
                existingItem.Quantity = qty;
            }
        }

        else
        {
            if (qty > 0)
            {
                var game = await _context.Games.Include(g => g.Media).FirstOrDefaultAsync(g => g.ID == gameId);
                if (game == null)
                    throw new ArgumentException("Game not found");
                var newItem = new BasketItem
                {
                    Basket = basket,
                    Game = game,
                    Quantity = qty,
                    UnitPrice = game.Price
                };
                _context.BasketItems.Add(newItem);
            }
        }
        await _context.SaveChangesAsync();
        return new BasketResponseDto
        {
            Items = basket.Items.Select(item => new BasketItemDto
            {
                GameId = item.Game.ID,
                GameName = item.Game.Name,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                ImageUrl = item.Game.Media.Where(i => i.IsPrimary).Select(m => m.Url).FirstOrDefault()
            }).ToList(),
            Total = basket.Items.Sum(item => item.UnitPrice * item.Quantity)
        };
    }

    public async Task<BasketResponseDto> GetBasketAsync(int userId)
    {
        var basket = await _context.Baskets
            .Include(b => b.Items)
                .ThenInclude(i => i.Game)
            .ThenInclude(g => g.Media)
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (basket == null)
        {
            return new BasketResponseDto
            {
                Items = [],
                Total = 0
            };
        }

        var items = basket.Items.Select(item => new BasketItemDto
        {
            GameId = item.Game.ID,
            GameName = item.Game.Name,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            ImageUrl = item.Game.Media
                .Where(m => m.IsPrimary)
                .Select(m => m.Url)
                .FirstOrDefault()
        }).ToList();

        var total = basket.Items.Sum(i => i.Quantity * i.UnitPrice);

        return new BasketResponseDto
        {
            Items = items,
            Total = total
        };
    }


    public async Task<BasketResponseDto> ClearBasketAsync(int userId)
    {
        var basket = await _context.Baskets.Include(g => g.Items).ThenInclude(g => g.Game).ThenInclude(g => g.Media).FirstOrDefaultAsync(u => u.UserId == userId);

        if (basket == null)
            return new BasketResponseDto
            {
                Items = [],
                Total = 0
            };
        _context.BasketItems.RemoveRange(basket.Items);
        await _context.SaveChangesAsync();

        return new BasketResponseDto
        {
            Items = [],
            Total = 0
        };
    }
}
