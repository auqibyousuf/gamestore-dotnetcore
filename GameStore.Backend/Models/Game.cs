using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Backend.Models;

public class Game
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int ID { get; private set; }   // 🔒 private setter
  [Required]
  public string Name { get; set; } = null!;

  // Adding refrence to genre modal
  public Genre? Genre { get; set; }
// key for Games from genre
  public int GenreID { get; set; }

  [Range(0.01, double.MaxValue, ErrorMessage = "Price Must be greater than 0")]
  public decimal Price { get; set; }

  [Required]
  public DateOnly ReleaseDate { get; set; }

  public ICollection<GameMedia> Media {get; set;} = [];
}
