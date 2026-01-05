using System;

namespace GameStore.Backend.Models;

public class GameMedia
{

    public int Id { get; set; }
    public int GameId { get; set; }
    public string Url { get; set; } = null!;
    public string FileType { get; set; } = null!;
    public string OriginalFileName { get; set; } = null!;
    public Game Game { get; set; } = null!;
    public bool IsPrimary { get; set; }

}
