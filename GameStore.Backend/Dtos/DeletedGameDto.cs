using System;

namespace GameStore.Backend.Dtos;

public class DeletedGameDto
{
    public string Message { get; init; } = string.Empty;
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
