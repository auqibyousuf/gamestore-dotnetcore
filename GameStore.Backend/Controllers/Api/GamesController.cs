using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GameStore.Backend.Data;
using GameStore.Backend.Dtos;
using GameStore.Backend.Dtos.Common;
using GameStore.Backend.Enums;
using GameStore.Backend.Models;
using GameStore.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace GameStore.Backend.Controllers;

[ApiController]

[Route("api/games")]
public class GamesController(GameService gameService) : ControllerBase
{

  private readonly GameService _gameService = gameService;

  // GET Games
  [HttpGet]
  public async Task<ActionResult<PagedResultDto<GameListDto>>> GetGames(int? genreID, string? search, decimal? minPrice,
decimal? maxPrice, SortBy sortBy = SortBy.Id, OrderBy orderBy = OrderBy.Asc, int page = 1, int limit = 10, SearchMode searchMode = SearchMode.Contains)
  {
    var fetchAllGames = await _gameService.GetAllGamesAsync(genreID, search, minPrice, maxPrice, sortBy, orderBy, page, limit, searchMode);
    return Ok(BaseResponse<PagedResultDto<GameListDto>>.Ok(fetchAllGames, "Games Fetched Successfully"));
  }

  //GET by id
  [HttpGet("{id}")]
  public async Task<ActionResult<GameDetailsDto>> GetGame(int id)
  {

    var getGameById = await _gameService.GetGameByIdAsync(id);

    return Ok(BaseResponse<GameDetailsDto>.Ok(getGameById, "Game Fetched:{id}"));
  }

  //POST

  [HttpPost]
  public async Task<ActionResult<GameDetailsDto>> CreateGame(CreateGameDto dto)
  {
    var createdGame = await _gameService.CreateGameAsync(dto);
    return CreatedAtAction(nameof(GetGame), new { id = createdGame.ID }, BaseResponse<GameDetailsDto>.Ok(createdGame, "Game created successfully"));
  }


  // PUT
  [HttpPut("{id}")]
  public async Task<ActionResult<UpdateGameDto>> UpdateGame(int id, UpdateGameDto dto)
  {
    var game = await _gameService.UpdateGameAsync(id, dto);
    return Ok(BaseResponse<UpdateGameDto>.Ok(game, "Details Updated Successfully"));
  }


  [Authorize(Policy = "AdminOnly")]
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteGame(int id)
  {
    var game = await _gameService.DeleteGameAsync(id);
    return Ok(BaseResponse<DeletedGameDto>.Ok(game, $"Successfully Deleted:{game}"));
  }

  [HttpPost("{Id}/media")]
  public async Task<ActionResult<GameMediaResponseDto>> AddGameMedia(int Id, [FromForm] UploadGameMediaDto dto)
  {
    var media = await _gameService.AddGameMediaAsync(Id, dto);
    return Ok(BaseResponse<GameMediaResponseDto>.Ok(media, $"Successfully Added:{media}"));
  }
}
