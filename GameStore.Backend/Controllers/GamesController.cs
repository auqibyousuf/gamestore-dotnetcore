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

  private IQueryable<Game> BaseGameQuery()
  {
    return _context.Games.AsNoTracking().Include(g => g.Genre);
  }

  private static IQueryable<Game> ApplySorting(
    IQueryable<Game> query, SortBy sortBy, OrderBy orderBy
  )
  {
    return sortBy switch
    {
      SortBy.Price => orderBy == OrderBy.Desc ? query.OrderByDescending(order => order.Price) : query.OrderBy(order => order.Price),
      SortBy.ReleaseDate => orderBy == OrderBy.Desc ? query.OrderByDescending(g => g.ReleaseDate) : query.OrderBy(g => g.ReleaseDate),
      SortBy.Name => orderBy == OrderBy.Desc ? query.OrderByDescending(g => g.Name) : query.OrderBy(g => g.Name),
      SortBy.Id => orderBy == OrderBy.Desc ? query.OrderByDescending(g => g.ID) : query.OrderBy(g => g.ID),
      _ => query.OrderBy(order => order.ID)

    };
  }

  private static IQueryable<Game> ApplyPagination(IQueryable<Game> query, int page = 1, int limit = 10)
  {
    return query.Skip((page - 1) * limit).Take(limit);
  }

  // GET Games
  [HttpGet]
  // Various Operations on GET Data
  public async Task<ActionResult<GameListDto>> GetGames(int? genreID, string? search, decimal? minPrice,
decimal? maxPrice, SortBy sortBy = SortBy.Id, OrderBy orderBy = OrderBy.Asc, int page = 1, int limit = 10)
  {
    var query = BaseGameQuery().AsQueryable();

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

    var totalCount = await query.CountAsync();
    query = ApplySorting(query, sortBy, orderBy);
    query = ApplyPagination(query, page, limit);

    var games = await query.Select(game => new GameListDto
    {
      Name = game.Name,
      ID = game.ID,
      Price = game.Price,
      GenreName = game.Genre!.Name,
    }).ToListAsync();
    return Ok(new
    {
      page,
      limit,
      totalCount,
      totalPages = (int)Math.Ceiling(totalCount / (double)limit),
      items = games
    });
  }

  //GET by id
  [HttpGet("{id}")]
  public async Task<ActionResult<GameDetailsDto>> GetGame(int id)
  {
    var game = await _context.Games.Include(g => g.Genre).Where(g => g.ID == id).Select(g => new GameDetailsDto
    {
      Name = g.Name,
      ID = g.ID,
      Price = g.Price,
      ReleaseDate = g.ReleaseDate,
      GenreName = g.Genre!.Name,
      GenreID = g.Genre.ID
    }).FirstOrDefaultAsync();

    if (game is null)
      return NotFound();
    return Ok(game);
  }

  //POST
  [HttpPost]
  public async Task<ActionResult<GameDetailsDto>> CreateGame(CreateGameDto dto)
  {
    var genreExists = await _context.Genres.AnyAsync(genre => genre.ID == dto.GenreID);
    if (!genreExists)
      return BadRequest("Invalid GenreID");

    var game = new Game
    {
      Name = dto.Name,
      Price = dto.Price,
      ReleaseDate = dto.ReleaseDate,
      GenreID = dto.GenreID,
    };


    _context.Games.Add(game);
    await _context.SaveChangesAsync();


    var createdGame = BaseGameQuery()
    .Where(g => g.ID == game.ID)
    .Select(g => new GameDetailsDto
    {
      ID = g.ID,
      Name = g.Name,
      Price = g.Price,
      ReleaseDate = g.ReleaseDate,
      GenreID = g.GenreID,
      GenreName = g.Genre!.Name
    })
    .FirstAsync();

    return CreatedAtAction(nameof(GetGame), new { id = createdGame.Id }, createdGame);
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
