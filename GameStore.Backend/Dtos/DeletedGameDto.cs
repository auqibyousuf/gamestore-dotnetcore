using System;

namespace GameStore.Backend.Dtos;

public class DeletedGameDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Genre { get; set; } = string.Empty;

    public DateTime DeletedAt { get; set; }
}
