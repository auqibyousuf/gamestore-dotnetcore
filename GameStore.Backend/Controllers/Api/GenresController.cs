using System;
using GameStore.Backend.Data;
using GameStore.Backend.Dtos;
using GameStore.Backend.Dtos.Common;
using GameStore.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Backend.Controllers;

[ApiController]
[Route("api/genres")]
public class GenresController(GameStoreContext context) : ControllerBase
{
  private readonly GameStoreContext _context = context;

  // PUT: api/genres
  [HttpGet]
  public async Task<ActionResult<BaseResponse<List<Genre>>>> GetGenres()
  {
    var genre = await _context.Genres.AsNoTracking().ToListAsync();
    return Ok(BaseResponse<object>.Ok(genre, "Genre Fetched"));
  }

  // GET: api/genres/5
  [HttpGet("{id}")]
  public async Task<ActionResult<Genre>> GetGenre(int id)
  {
    var genre = await _context.Genres.FindAsync(id);
    return genre is null ? NotFound(BaseResponse<object>.Fail("Genre Not found")) : Ok(BaseResponse<object>.Ok(genre, "Genre Fetched"));
  }

  // POST: api/genres/5
  [HttpPost]
  public async Task<ActionResult<Genre>> CreateGenre(CreateGenreDto dto)
  {

    var genreExists = await _context.Genres.AnyAsync(g => g.Name == dto.Name);
    if (genreExists)
      return BadRequest(BaseResponse<object>.Fail("Genre already exists"));
    var genre = new Genre
    {
      Name = dto.Name
    };
    _context.Genres.Add(genre);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetGenre), new { id = genre.ID }, BaseResponse<Genre>.Ok(genre, "Genre created"));
  }

  // PUT: api/genres/5
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateGenre(int id, UpdateGenreDto dto)
  {
    if (id != dto.ID)
      return BadRequest(BaseResponse<object>.Fail("Id not found"));

    var existingGenre = await _context.Genres.FindAsync(id);
    if (existingGenre is null)
      return NotFound(BaseResponse<object>.Fail("Genre Not found"));
    existingGenre.Name = dto.Name;
    await _context.SaveChangesAsync();
    return Ok(BaseResponse<object>.Ok(new
    {
      existingGenre.ID,
      existingGenre.Name
    }, "Genre Updated"));
  }

  // DELETE: api/genres/5
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteGenre(int id)
  {
    var genre = await _context.Genres.FindAsync(id);
    if (genre is null)
      return NotFound(BaseResponse<object>.Fail("Genre not found"));
    _context.Genres.Remove(genre);
    await _context.SaveChangesAsync();
    return Ok(BaseResponse<object>.Ok(new
    {
      genre.ID,
      genre.Name
    }, "Deleted Successfully"));
  }
}
