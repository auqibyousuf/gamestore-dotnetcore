using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Backend.Models;

public class GameMedia
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int GameId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public Game Game { get; set; } = null!;
    public bool IsPrimary { get; set; }

}
