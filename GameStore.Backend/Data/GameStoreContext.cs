using System;
using GameStore.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Backend.Data;

public class GameStoreContext(DbContextOptions<GameStoreContext> options) : DbContext(options)
{
  public DbSet<Game> Games => Set<Game>();
  public DbSet<Genre> Genres => Set<Genre>();

  public DbSet<GameMedia> GameMedia => Set<GameMedia>();

  //User context
  public DbSet<User> Users => Set<User>();

  public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
  public DbSet<Basket> Baskets => Set<Basket>();
  public DbSet<BasketItem> BasketItems => Set<BasketItem>();
  public DbSet<Order> Orders => Set<Order>();
  public DbSet<OrderItem> OrderItems => Set<OrderItem>();
  public DbSet<Payment> Payments => Set<Payment>();
}
