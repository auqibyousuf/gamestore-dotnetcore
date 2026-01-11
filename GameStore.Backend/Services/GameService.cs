using System;
using GameStore.Backend.Data;
using GameStore.Backend.Dtos;
using GameStore.Backend.Enums;
using GameStore.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GameStore.Backend.Services;

public class GameService(GameStoreContext context, ILogger<GameService> logger, IMemoryCache memory, IFileStorageService fileStorageService)
{

    private readonly GameStoreContext _context = context;
    private readonly IMemoryCache _memory = memory;
    private readonly ILogger<GameService> _logger = logger;
    private readonly IFileStorageService _fileStorageService = fileStorageService;


    private static string BuildGamesCacheKey(
        int? genreID,
        string? search,
        decimal? minPrice,
        decimal? maxPrice,
        SortBy sortBy,
        OrderBy orderBy,
        int page,
        int limit,
        SearchMode searchMode
    )
    {
        return $"games:list:" +
        $"genre={genreID}:" +
        $"search={search ?? "null"}:" +
        $"min={minPrice}:" +
        $"max={maxPrice}:" +
        $"sort={sortBy}:" +
        $"order={orderBy}:" +
        $"page={page}:" +
        $"limit={limit}:" +
        $"mode={searchMode}";
    }
    private IQueryable<Game> BaseGameQuery()
    {
        return _context.Games.AsNoTracking().Include(g => g.Genre).Include(g => g.Media);
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
    private static IQueryable<Game> ApplySorting(IQueryable<Game> query, SortBy sortBy, OrderBy orderBy)
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
    public async Task<PagedResultDto<GameListDto>> GetAllGamesAsync(int? genreID, string? search, decimal? minPrice, decimal? maxPrice, SortBy sortBy = SortBy.Id, OrderBy orderBy = OrderBy.Asc, int page = 1, int limit = 10, SearchMode searchMode = SearchMode.Contains)
    {
        var cacheKey = BuildGamesCacheKey(genreID, search, minPrice, maxPrice, sortBy, orderBy, page, limit, searchMode);

        if (_memory.TryGetValue(cacheKey, out PagedResultDto<GameListDto>? cached))
        {
            _logger.LogInformation("Games cache HIT: {CacheKey}", cacheKey);
            return cached!;
        }


        _logger.LogInformation("Fetching games list");
        var query = BaseGameQuery().AsQueryable();
        query = ApplyFilters(query, minPrice, maxPrice, genreID);
        query = ApplySearching(query, search ?? string.Empty, searchMode);
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

        var result = new PagedResultDto<GameListDto>
        {
            Page = page,
            Limit = limit,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)limit),
            Items = games
        };

        _memory.Set(
        cacheKey,
        result,
        TimeSpan.FromMinutes(5)
    );
        return result;
    }


    public async Task<GameDetailsDto> GetGameByIdAsync(int id)
    {
        var game = await _context.Games.Include(g => g.Genre).Include(g => g.Media).Where(g => g.ID == id).Select(g => new GameDetailsDto
        {
            Name = g.Name,
            ID = g.ID,
            Price = g.Price,
            ReleaseDate = g.ReleaseDate,
            GenreName = g.Genre!.Name,
            GenreID = g.Genre.ID,
            ImageUrl = g.Media.Where(m => m.IsPrimary).Select(m => m.Url).FirstOrDefault()
        }).FirstOrDefaultAsync() ?? throw new InvalidOperationException("No game found");

        return game;
    }

    //POST

    public async Task<GameDetailsDto> CreateGameAsync(CreateGameDto dto)
    {
        var genreExists = await _context.Genres.AnyAsync(genre => genre.ID == dto.GenreID);
        if (!genreExists)
            throw new InvalidOperationException("Invalid GenreID");

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

        return createdGame;
    }
    public async Task<UpdateGameDto> UpdateGameAsync(int id, UpdateGameDto dto)
    {
        if (id != dto.ID)
            throw new InvalidOperationException("Invalid ID");

        var genreExists = await _context.Genres.AnyAsync(g => g.ID == dto.GenreID);

        if (!genreExists)
            throw new InvalidOperationException("Invalid GenreID");

        var game = await _context.Games.FindAsync(id) ?? throw new KeyNotFoundException("Game Not found");

        game.Name = dto.Name;
        game.GenreID = dto.GenreID;
        game.Price = dto.Price;
        game.ReleaseDate = dto.ReleaseDate;


        await _context.SaveChangesAsync();

        return new UpdateGameDto
        {
            ID = game.ID,
            Name = game.Name,
            GenreID = game.GenreID,
            Price = game.Price,
            ReleaseDate = game.ReleaseDate,
        };
    }



    public async Task<DeletedGameDto> DeleteGameAsync(int id)
    {
        var deleteGame = await _context.Games.Include(g => g.Genre).FirstOrDefaultAsync(g => g.ID == id) ?? throw new KeyNotFoundException("Game Not found");

        var deletedGame = new DeletedGameDto
        {
            Id = deleteGame.ID,
            Name = deleteGame.Name,
            Genre = deleteGame.Genre!.Name,
            DeletedAt = DateTime.UtcNow
        };

        _context.Games.Remove(deleteGame);
        await _context.SaveChangesAsync();
        return deletedGame;
    }

    public async Task<GameMediaResponseDto> AddGameMediaAsync(int gameId, UploadGameMediaDto dto)
    {
        var gameExists = await _context.Games.AnyAsync(g => g.ID == gameId);

        if (!gameExists)
            throw new KeyNotFoundException("Game not found");
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

        return new GameMediaResponseDto
        {
            Id = media.Id,
            Url = media.Url,
            OriginalFileName = media.OriginalFileName,
            IsPrimary = media.IsPrimary
        };
    }
}
