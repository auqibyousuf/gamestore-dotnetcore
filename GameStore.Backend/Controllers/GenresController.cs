using System;
using GameStore.Backend.Data;
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
  public async Task<ActionResult<List<Genre>>> GetGenres()
  {
    return Ok(await _context.Genres.ToListAsync());
  }

  // GET: api/genres/5
  [HttpGet("{id}")]
  public async Task<ActionResult<Genre>> GetGenre(int id)
  {
    var genre = await _context.Genres.FindAsync(id);
    return genre is null ? NotFound() : Ok(genre);
  }

  // POST: api/genres/5
  [HttpPost]
  public async Task<ActionResult<Genre>> CreateGenre(Genre genre)
  {
    _context.Genres.Add(genre);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetGenres), new { id = genre.ID }, genre);
  }

  // PUT: api/genres/5
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateGenre(int id, Genre updatedGenre)
  {
    if (id != updatedGenre.ID)
      return BadRequest();

    var existingGenre = await _context.Genres.FindAsync(id);
    if (existingGenre is null)
      return NotFound();

    existingGenre.Name = updatedGenre.Name;
    await _context.SaveChangesAsync();
    return NoContent();
  }

  // DELETE: api/genres/5
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteGenre(int id)
  {
    var genre = await _context.Genres.FindAsync(id);
    if (genre is null)
      return NotFound();
    _context.Genres.Remove(genre);
    await _context.SaveChangesAsync();
    return NoContent();
  }
}
