using System;

namespace GameStore.Backend.Dtos;

public class GameListDto
{
    public int ID { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string GenreName { get; set; } = "";
    public string? ImageUrl { get; set; }
}
