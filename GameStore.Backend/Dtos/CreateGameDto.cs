using System;
using System.ComponentModel.DataAnnotations;

namespace GameStore.Backend.Dtos;

public class CreateGameDto
{
  [Required]
  public string Name { get; set; } = null!;
  [Range(0.01, double.MaxValue)]
  public decimal Price { get; set; }

  [Required]
  public DateOnly ReleaseDate { get; set; }

  [Required]
  public int GenreID { get; set; }
}
