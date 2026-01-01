using System;
using GameStore.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Backend.Data;

public class GameStoreContext(DbContextOptions<GameStoreContext> options) : DbContext(options)
{
  public DbSet<Game> Games => Set<Game>();
  public DbSet<Genre> Genres => Set<Genre>();

  //User context
  public DbSet<User> Users => Set<User>();
}
