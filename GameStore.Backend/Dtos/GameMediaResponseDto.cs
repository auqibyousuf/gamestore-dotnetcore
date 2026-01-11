using System;

namespace GameStore.Backend.Dtos;

public class GameMediaResponseDto
{

    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public string OriginalFileName { get; set; } = null!;
    public bool IsPrimary { get; set; }

}
