using System;
using System.ComponentModel.DataAnnotations;
using GameStore.Backend.Models;

namespace GameStore.Backend.Dtos;

public class UpdateGameDto
{
  public int ID { get; set; }
  [Required]
  public string Name { get; set; } = null!;
  [Range(1, double.MaxValue)]
  public decimal Price { get; set; }

  [Required]
  public DateOnly ReleaseDate { get; set; }

  [Required]
  public int GenreID { get; set; }

  // public List<UpdateGameMediaDto> Media { get; set; } = [];
}
