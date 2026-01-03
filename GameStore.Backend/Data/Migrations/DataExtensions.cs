using GameStore.Backend.Data;
using GameStore.Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class DataExtensions
{
  // 1️⃣ Automatically apply migrations
  public static void MigrateDb(this WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<GameStoreContext>();
    context.Database.Migrate();


    // ----------- GENRES -----------
    if (!context.Genres.Any())
    {
      context.Genres.AddRange(
          new Genre { Name = "Action" },
          new Genre { Name = "RPG" },
          new Genre { Name = "Sports" },
          new Genre { Name = "Racing" },
          new Genre { Name = "Strategy" },
          new Genre { Name = "Adventure" }
      );
      context.SaveChanges();
    }

    // ----------- GAMES -----------
    if (!context.Games.Any())
    {
      var genreMap = context.Genres
          .ToDictionary(g => g.Name, g => g.ID);

      context.Games.AddRange(

          // Action
          new Game { Name = "God of War", Price = 49.99m, ReleaseDate = new(2018, 4, 20), GenreID = genreMap["Action"] },
          new Game { Name = "Sekiro", Price = 59.99m, ReleaseDate = new(2019, 3, 22), GenreID = genreMap["Action"] },
          new Game { Name = "Devil May Cry 5", Price = 39.99m, ReleaseDate = new(2019, 3, 8), GenreID = genreMap["Action"] },

          // RPG
          new Game { Name = "Elden Ring", Price = 59.99m, ReleaseDate = new(2022, 2, 25), GenreID = genreMap["RPG"] },
          new Game { Name = "Skyrim", Price = 39.99m, ReleaseDate = new(2011, 11, 11), GenreID = genreMap["RPG"] },
          new Game { Name = "The Witcher 3", Price = 29.99m, ReleaseDate = new(2015, 5, 19), GenreID = genreMap["RPG"] },

          // Sports
          new Game { Name = "FIFA 24", Price = 69.99m, ReleaseDate = new(2023, 9, 29), GenreID = genreMap["Sports"] },
          new Game { Name = "NBA 2K24", Price = 69.99m, ReleaseDate = new(2023, 9, 8), GenreID = genreMap["Sports"] },
          new Game { Name = "PES 2021", Price = 29.99m, ReleaseDate = new(2020, 9, 15), GenreID = genreMap["Sports"] },

          // Racing
          new Game { Name = "Forza Horizon 5", Price = 54.99m, ReleaseDate = new(2021, 11, 9), GenreID = genreMap["Racing"] },
          new Game { Name = "Gran Turismo 7", Price = 69.99m, ReleaseDate = new(2022, 3, 4), GenreID = genreMap["Racing"] },
          new Game { Name = "Need for Speed Heat", Price = 39.99m, ReleaseDate = new(2019, 11, 8), GenreID = genreMap["Racing"] },

          // Strategy
          new Game { Name = "Civilization VI", Price = 59.99m, ReleaseDate = new(2016, 10, 21), GenreID = genreMap["Strategy"] },
          new Game { Name = "Age of Empires IV", Price = 59.99m, ReleaseDate = new(2021, 10, 28), GenreID = genreMap["Strategy"] },

          // Adventure
          new Game { Name = "The Last of Us", Price = 29.99m, ReleaseDate = new(2013, 6, 14), GenreID = genreMap["Adventure"] }
      );

      context.SaveChanges();
    }

    // ----------- ADMIN USER -----------
    if (!context.Users.Any(u => u.Role == "Admin"))
    {
      var hasher = new PasswordHasher<User>();

      var admin = new User
      {
        Name = "Admin",
        Email = "admin@admin.com",
        Role = "Admin"
      };

      admin.PasswordHash = hasher.HashPassword(admin, "admin@123");
      // ----------- Normal USER -----------
      var user = new User
      {
        Name = "John Doe",
        Email = "john@example.com",
        Role = "User"
      };
      user.PasswordHash = hasher.HashPassword(user, "user@123");

      var user1= new User
      {
        Name = "John Doe1",
        Email = "john@example.com",
        Role = "User"
      };
      user1.PasswordHash = hasher.HashPassword(user, "user@123");

      var user2 = new User
      {
        Name = "John Doe2",
        Email = "john@example.com",
        Role = "User"
      };
      user2.PasswordHash = hasher.HashPassword(user, "user@123");
      context.Users.AddRange(admin, user,user1,user2);
      context.SaveChanges();
    }



  }

  public static void AddGameStoreDb(this WebApplicationBuilder builder)
  {
    var connString = builder.Configuration.GetConnectionString("GameStore");

    builder.Services.AddDbContext<GameStoreContext>(options =>
        options.UseSqlite(connString));
  }
}
