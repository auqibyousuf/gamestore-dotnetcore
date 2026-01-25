using GameStore.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Backend.Controllers
{
    public class HomeController(GameService gameService) : Controller
    {
        private readonly GameService _gameService = gameService;
        
        // GET: HomeController
        public async Task<IActionResult> Index()
        {
            var games = await _gameService.GetAllGamesAsync(
                genreID: null,
                search: null,
                minPrice: null,
                maxPrice: null
            );

            return View(games.Items);
        }

    }
}
