using GameStore.Backend.Services;
using GameStore.Backend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Backend.Controllers.Mvc;

public class GamesController(GameService gameService, ILogger<GamesController> logger) : Controller
{
    private readonly GameService _gameService = gameService;
    private readonly ILogger<GamesController> _logger = logger;

    // GET /Games
    public async Task<IActionResult> Index()
    {
        var games = await _gameService.GetAllGamesAsync(
            genreID: null,
            search: null,
            minPrice: null,
            maxPrice: null
        );

        var vm = new GameIndexViewModel
        {
            Games = games.Items
        };

        return View(vm);
    }

    public async Task<IActionResult> Genres()
    {
        var games = await _gameService.GetAllGamesAsync(
            genreID: null,
            search: null,
            minPrice: null,
            maxPrice: null
        );

        var vm = new GameIndexViewModel
        {
            Games = games.Items
        };

        return View(vm);
    }

    // POST /Games/CreateGame
    [HttpPost]
    public async Task<IActionResult> CreateGame(GameIndexViewModel model)
    {
        _logger.LogInformation("CreateGame POST hit");

        if (!ModelState.IsValid)
        {
            model.Games = (await _gameService
                .GetAllGamesAsync(null, null, null, null)).Items;

            return View("Index", model);
        }

        await _gameService.CreateGameAsync(model.CreateGame);

        TempData["SuccessMessage"] = "Game added successfully 🎉";

        return RedirectToAction(nameof(Index));
    }
}
