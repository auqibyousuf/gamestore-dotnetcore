using GameStore.Backend.Data;
using GameStore.Backend.Dtos;
using GameStore.Backend.Enums;
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
  // Various Operations on GET Data
  public async Task<IActionResult> GetGames(int? genreID, string? search, decimal? minPrice,
decimal? maxPrice, SortBy sortBy = SortBy.Id, OrderBy orderBy = OrderBy.Asc, int page = 1, int limit = 10)
  {
    var query = _context.Games.Include(g => g.Genre).AsQueryable();

    var totalCount = await query.CountAsync();

    if (page <= 0 || limit <= 0)
    {
      return BadRequest("Error Page number and limit must be greater than 0");
    }
    if (limit > 50)
    {
      limit = 50;
    }
    if (genreID.HasValue)
    {
      query = query.Where(g => g.GenreID == genreID.Value);
    }

    if (minPrice.HasValue)
    {
      query = query.Where(g => g.Price >= minPrice.Value);
    }

    if (maxPrice.HasValue)
    {
      query = query.Where(g => g.Price <= maxPrice.Value);
    }

    //Searching

    if (!string.IsNullOrWhiteSpace(search))
    {
      query = query.Where(g => g.Name.ToLower().Contains(search.ToLower()));
    }


    var sortedQuery = query = sortBy switch
    {
      SortBy.Price => orderBy == OrderBy.Desc ? query.OrderByDescending(g => g.Price) : query.OrderBy(g => g.Price),
      SortBy.ReleaseDate => orderBy == OrderBy.Desc ? query.OrderByDescending(g => g.ReleaseDate) : query.OrderBy(g => g.ReleaseDate),
      SortBy.Name => orderBy == OrderBy.Desc ? query.OrderByDescending(g => g.Name) : query.OrderBy(g => g.Name),
      SortBy.Id => orderBy == OrderBy.Desc ? query.OrderByDescending(g => g.ID) : query.OrderBy(g => g.ID),
      _ => query.OrderBy(g => g.ID)
    };

    var sortedPagination = sortedQuery.Skip((page - 1) * limit).Take(limit);
    var fetchGames = await sortedPagination.Select(game => new ResponseGameDto
    {
      Name = game.Name,
      ID = game.ID,
      Price = game.Price,
      ReleaseDate = game.ReleaseDate,
      GenreName = game.Genre!.Name,
      GenreID = game.Genre.ID
    }).ToListAsync();
    return Ok(new
    {
      page,
      limit,
      totalCount,
      totalPages = (int)Math.Ceiling(totalCount / (double)limit),
      items = fetchGames
    });
  }

  //GET by id
  [HttpGet("{id}")]
  public async Task<ActionResult<Game>> GetGame(int id)
  {
    var gameByID = await _context.Games.FindAsync(id);
    if (gameByID == null)
      return NotFound();
    return Ok(gameByID);
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
