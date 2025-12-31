using GameStore.Backend.Data;
using GameStore.Backend.Dtos;
using GameStore.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Backend.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController(GameStoreContext context) : ControllerBase
{

  private readonly GameStoreContext _context = context;

  // GET Games
  [HttpGet]
  public async Task<ActionResult<List<Game>>> GetGames()
  {
    var games = await _context.Games
    .Include(g => g.Genre)
    .Select(game => new ResponseGameDto
    {
      ID = game.ID,
      Name = game.Name,
      Price = game.Price,
      ReleaseDate = game.ReleaseDate,
      GenreID = game.GenreID,
      GenreName = game.Genre!.Name
    }).ToListAsync();
    return Ok(games);
  }

  //POST
  [HttpPost]
  public async Task<ActionResult<Game>> CreateGame(CreateGameDto dto)
  {
    var genreExists = await _context.Genres.AnyAsync(genre => genre.ID == dto.GenreID);
    if (!genreExists)
      return BadRequest("Invalid GenreID");

    var game = new Game
    {
      Name = dto.Name,
      Price = dto.Price,
      ReleaseDate = dto.ReleaseDate,
      GenreID = dto.GenreID
    };


    _context.Games.Add(game);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetGames), new { ID = game.ID }, game);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Game>> GetGame(int id)
  {
    var gameByID = await _context.Games.FindAsync(id);
    if (gameByID == null)
      return NotFound();
    return Ok(gameByID);
  }

  // PUT
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateGame(int id, UpdateGameDto dto)
  {
    if (id != dto.ID)
      return BadRequest();

    var genreExists = await _context.Genres.AnyAsync(g => g.ID == dto.GenreID);

    if (!genreExists)
      return BadRequest("Invalid GenreId");

    var existingGame = await _context.Games.FindAsync(id);
    if (existingGame == null)
      return NotFound();

    existingGame.Name = dto.Name;
    existingGame.GenreID = dto.GenreID;
    existingGame.Price = dto.Price;
    existingGame.ReleaseDate = dto.ReleaseDate;
    await _context.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteGame(int id)
  {
    var deleteGame = await _context.Games.FindAsync(id);
    if (deleteGame == null)
      return NotFound();

    _context.Games.Remove(deleteGame);
    await _context.SaveChangesAsync();
    return NoContent();
  }
}
