using System;

namespace GameStore.Backend.Dtos;

public class ResponseGameDto
{
  public int ID { get; set; }
  public string Name { get; set; } = null!;
  public decimal Price { get; set; }
  public DateOnly ReleaseDate { get; set; }
  public int GenreID { get; set; }
  public string GenreName { get; set; } = null!;
}
