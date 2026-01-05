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
public class GamesController(GameStoreContext context, ILogger<GamesController> logger, IFileStorageService fileStorageService) : ControllerBase
{

  private readonly GameStoreContext _context = context;
  private readonly ILogger<GamesController> _logger = logger;
  private readonly IFileStorageService _fileStorageService = fileStorageService;

  private IQueryable<Game> BaseGameQuery()
  {
    return _context.Games.AsNoTracking().Include(g => g.Genre).Include(g => g.Media); ;
  }


  private static IQueryable<Game> ApplyFilters(IQueryable<Game> query, decimal? minPrice, decimal? maxPrice, int? genreID)
  {
    if (genreID.HasValue)
    {
      query = query.Where(g => g.GenreID == genreID!.Value);
    }

    if (minPrice.HasValue)
    {
      query = query.Where(min => min.Price >= minPrice.Value);
    }

    if (maxPrice.HasValue)
    {
      query = query.Where(max => max.Price <= maxPrice.Value);
    }
    return query;
  }

  private static IQueryable<Game> ApplySearching(IQueryable<Game> query, string searchTerm, SearchMode searchMode = SearchMode.Contains)
  {
    if (string.IsNullOrWhiteSpace(searchTerm))
      return query;
    var term = searchTerm.Trim().ToLower();
    return searchMode switch
    {

      SearchMode.StartsWith =>
      query.Where(search => search.Name.ToLower().StartsWith(term) || search.Genre != null && search.Genre.Name.ToLower().StartsWith(term)),

      SearchMode.EndsWith =>
      query.Where(search =>
      search.Name.ToLower().EndsWith(term) || search.Genre != null &&
      search.Genre.Name.ToLower().EndsWith(term)),

      SearchMode.Exact => query.Where(search => search.Name.ToLower() == term || search.Genre != null && search.Genre.Name.ToLower() == term),

      _ => query.Where(search => search.Name.ToLower().Contains(term) || search.Genre != null && search.Genre.Name.Contains(term))
    };
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

    if (page <= 0 || limit <= 0)
    {
      return query;
    }
    if (limit > 50)
    {
      limit = 50;
    }
    return query.Skip((page - 1) * limit).Take(limit);
  }

  // GET Games
  [HttpGet]
  // Various Operations on GET Data
  public async Task<ActionResult<GameListDto>> GetGames(int? genreID, string? search, decimal? minPrice,
decimal? maxPrice, SortBy sortBy = SortBy.Id, OrderBy orderBy = OrderBy.Asc, int page = 1, int limit = 10, SearchMode searchMode = SearchMode.Contains)
  {
    _logger.LogInformation("Fetching games list");
    var query = BaseGameQuery().AsQueryable();
    query = ApplyFilters(query, minPrice, maxPrice, genreID);
    query = ApplySearching(query, search!, searchMode);
    query = ApplySorting(query, sortBy, orderBy);

    var totalCount = await query.CountAsync();
    query = ApplyPagination(query, page, limit);

    var games = await query.Select(game => new GameListDto
    {
      Name = game.Name,
      ID = game.ID,
      Price = game.Price,
      GenreName = game.Genre!.Name,
      ImageUrl = game.Media.Where(m => m.IsPrimary).Select(m => m.Url).FirstOrDefault()
    }).ToListAsync();
    return Ok(BaseResponse<object>.Ok(
      new
      {
        page,
        limit,
        totalCount,
        totalPages = (int)Math.Ceiling(totalCount / (double)limit),
        items = games
      }, "Games fetched successfully"));
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
      GenreID = g.Genre.ID,
      ImageUrl = g.Media.Where(m => m.IsPrimary).Select(m => m.Url).FirstOrDefault()
    }).FirstOrDefaultAsync();

    if (game is null)
      return NotFound(BaseResponse<object>.Fail("Game not found"));
    return Ok(BaseResponse<GameDetailsDto>.Ok(game, "Game Fetched Successfully"));
  }

  //POST

  [HttpPost]
  public async Task<ActionResult<GameDetailsDto>> CreateGame(CreateGameDto dto)
  {
    var genreExists = await _context.Genres.AnyAsync(genre => genre.ID == dto.GenreID);
    if (!genreExists)
      return BadRequest(BaseResponse<object>.Fail("Invalid GenreID"));

    var game = new Game
    {
      Name = dto.Name,
      Price = dto.Price,
      ReleaseDate = dto.ReleaseDate,
      GenreID = dto.GenreID,
    };


    _context.Games.Add(game);
    await _context.SaveChangesAsync();


    var createdGame = await BaseGameQuery()
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

    return CreatedAtAction(nameof(GetGame), new { id = createdGame.ID }, BaseResponse<GameDetailsDto>.Ok(createdGame, "Game created successfully"));
  }


  // PUT
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateGame(int id, UpdateGameDto dto)
  {
    if (id != dto.ID)
      return BadRequest();

    var genreExists = await _context.Genres.AnyAsync(g => g.ID == dto.GenreID);

    if (!genreExists)
      return BadRequest(BaseResponse<object>.Fail("Invalid GenreID"));

    var existingGame = await _context.Games.FindAsync(id);
    if (existingGame == null)
      return NotFound(BaseResponse<object>.Fail("Not found"));

    var existingGameDetails = new
    {
      existingGame.Name,
      existingGame.GenreID,
      existingGame.Price,
      existingGame.ReleaseDate
    };

    existingGame.Name = dto.Name;
    existingGame.GenreID = dto.GenreID;
    existingGame.Price = dto.Price;
    existingGame.ReleaseDate = dto.ReleaseDate;
    await _context.SaveChangesAsync();

    var newGameDetails = new
    {
      existingGame.Name,
      existingGame.GenreID,
      existingGame.Price,
      existingGame.ReleaseDate
    };

    return Ok(BaseResponse<object>.Ok(new
    {
      existingGameDetails,
      newGameDetails
    }, "Details Updated Successfully"));
  }

  [Authorize(Policy = "AdminOnly")]
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteGame(int id)
  {
    var deleteGame = await _context.Games.FindAsync(id);
    if (deleteGame == null)
      return NotFound(BaseResponse<object>.Fail("Not found"));

    var deletedGame = new
    {
      deleteGame.ID,
      deleteGame.Name,
      deleteGame.Genre,
    };

    _context.Games.Remove(deleteGame);
    await _context.SaveChangesAsync();
    return Ok(BaseResponse<object>.Ok(new DeletedGameDto
    {
      Id = deletedGame.ID,
      Name = deletedGame.Name
    }, $"Successfully Deleted:{deletedGame}"));
  }


  [Authorize]
  [HttpGet("jwt-test")]
  public IActionResult WhoAmI()
  {
    _logger.LogInformation("Is Authenticated: {Auth}", User.Identity?.IsAuthenticated);
    return Ok("Check logs");
  }


  [Authorize]
  [HttpGet("Claims")]

  public IActionResult ViewClaims()
  {
    foreach (var claim in User.Claims)
    {
      _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
    }
    return Ok("check logs");
  }

  [Authorize]
  [HttpGet("user info")]

  public IActionResult ViewUserDetails()
  {
    var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    var role = User.FindFirst(ClaimTypes.Role)?.Value;

    var email = User.FindFirst(ClaimTypes.Email)?.Value;

    _logger.LogInformation("UserID={userID} Role={role} Email={email}", userID, role, email);
    return Ok(new { userID, email, role });
  }

  [HttpPost("{gameId}/media")]
  public async Task<IActionResult> AddGameMedia(int gameId, [FromForm] UploadGameMediaDto dto)
  {
    var gameExists = await _context.Games.AnyAsync(g => g.ID == gameId);

    if (!gameExists)
      return NotFound("Game not found");
    var hasPrimary = await _context.GameMedia
        .AnyAsync(m => m.GameId == gameId && m.IsPrimary);
    var uploadResult = await _fileStorageService.SaveAsync(dto.File);

    var media = new GameMedia
    {
      GameId = gameId,
      Url = uploadResult.Url,
      OriginalFileName = dto.File.FileName,
      FileType = Path.GetExtension(dto.File.FileName).ToLower(),
      IsPrimary = !hasPrimary
    };

    _context.GameMedia.Add(media);
    await _context.SaveChangesAsync();

    return Ok(BaseResponse<object>.Ok(new
    {
      media.Id,
      media.Url,
      media.OriginalFileName
    }, "File Uploaded Successfully"));
  }
}
