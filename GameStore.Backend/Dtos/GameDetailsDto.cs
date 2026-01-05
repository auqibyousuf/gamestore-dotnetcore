using System;

namespace GameStore.Backend.Dtos;

public class GameDetailsDto
{
    public int ID { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public string GenreName { get; set; } = "";
    public int GenreID { get; set; }

    public string? ImageUrl { get; set; }
}
