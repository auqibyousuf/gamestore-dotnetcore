using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Backend.Models;

public class Genre
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int ID { get; set; }
    public required string  Name { get; set; }
}
