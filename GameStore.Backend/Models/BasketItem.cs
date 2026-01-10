using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStore.Backend.Models;

public class BasketItem
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int Id { get; set; }
    public Basket Basket { get; set; } = null!;
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

}
