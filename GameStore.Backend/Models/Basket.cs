using System;

namespace GameStore.Backend.Models;

public class Basket
{

    public int ID { get; set; }
    public User User { get; set; } = null!;
    public int UserId { get; set; }

    public ICollection<BasketItem> Items { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

}
