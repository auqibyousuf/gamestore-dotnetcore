using System;

namespace GameStore.Backend.Dtos;

public class CreateGenreDto
{
    public int ID { get; set; }
    public required string Name { get; set; }
}
