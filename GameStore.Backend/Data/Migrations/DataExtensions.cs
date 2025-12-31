using GameStore.Backend.Data;
using GameStore.Backend.Models;
using Microsoft.EntityFrameworkCore;

public static class DataExtensions
{
  // 1️⃣ Automatically apply migrations
  public static void MigrateDb(this WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<GameStoreContext>();
    dbContext.Database.Migrate();

  }

  // 2️⃣ Register DbContext + seed data
  public static void AddGameStoreDb(this WebApplicationBuilder builder)
  {
    var connString = builder.Configuration.GetConnectionString("GameStore");

    builder.Services.AddDbContext<GameStoreContext>(options =>
        options.UseSqlite(connString)
        .UseSeeding((context, _) =>
        {
          // Seed Genres (lookup data)
          if (!context.Set<Genre>().Any())
          {
            // 🚨 STEP 1: If ANY data exists → STOP
            if (context.Set<Genre>().Any() || context.Set<Game>().Any())
              return;

            // ✅ STEP 2: Insert GENRES (no IDs)
            context.Set<Genre>().AddRange(
                new Genre { Name = "Action" },
                new Genre { Name = "RPG" },
                new Genre { Name = "Sports" },
                new Genre { Name = "Racing" },
                new Genre { Name = "Strategy" },
                new Genre { Name = "Adventure" }
            );

            // IMPORTANT: Save so IDs are generated
            context.SaveChanges();

            // ✅ STEP 3: Build genreMap (Name → Id)
            var genreMap = context.Set<Genre>()
                .ToDictionary(genre => genre.Name, genre => genre.ID);

            // ✅ STEP 4: Insert GAMES using genreMap
            context.Set<Game>().AddRange(

                // Action
                new Game { Name = "God of War", Price = 49.99m, ReleaseDate = new DateOnly(2018, 4, 20), GenreID = genreMap["Action"] },
                new Game { Name = "Devil May Cry 5", Price = 39.99m, ReleaseDate = new DateOnly(2019, 3, 8), GenreID = genreMap["Action"] },
                new Game { Name = "Sekiro", Price = 59.99m, ReleaseDate = new DateOnly(2019, 3, 22), GenreID = genreMap["Action"] },

                // RPG
                new Game { Name = "Elden Ring", Price = 59.99m, ReleaseDate = new DateOnly(2022, 2, 25), GenreID = genreMap["RPG"] },
                new Game { Name = "Skyrim", Price = 39.99m, ReleaseDate = new DateOnly(2011, 11, 11), GenreID = genreMap["RPG"] },
                new Game { Name = "Witcher 3", Price = 29.99m, ReleaseDate = new DateOnly(2015, 5, 19), GenreID = genreMap["RPG"] },

                // Sports
                new Game { Name = "FIFA 24", Price = 69.99m, ReleaseDate = new DateOnly(2023, 9, 29), GenreID = genreMap["Sports"] },
                new Game { Name = "NBA 2K24", Price = 69.99m, ReleaseDate = new DateOnly(2023, 9, 8), GenreID = genreMap["Sports"] },
                new Game { Name = "PES 2021", Price = 29.99m, ReleaseDate = new DateOnly(2020, 9, 15), GenreID = genreMap["Sports"] },

                // Racing
                new Game { Name = "Forza Horizon 5", Price = 54.99m, ReleaseDate = new DateOnly(2021, 11, 9), GenreID = genreMap["Racing"] },
                new Game { Name = "Gran Turismo 7", Price = 69.99m, ReleaseDate = new DateOnly(2022, 3, 4), GenreID = genreMap["Racing"] },
                new Game { Name = "Need for Speed Heat", Price = 39.99m, ReleaseDate = new DateOnly(2019, 11, 8), GenreID = genreMap["Racing"] },

                // Strategy
                new Game { Name = "Civilization VI", Price = 59.99m, ReleaseDate = new DateOnly(2016, 10, 21), GenreID = genreMap["Strategy"] },
                new Game { Name = "Age of Empires IV", Price = 59.99m, ReleaseDate = new DateOnly(2021, 10, 28), GenreID = genreMap["Strategy"] },
                new Game { Name = "StarCraft II", Price = 0m, ReleaseDate = new DateOnly(2010, 7, 27), GenreID = genreMap["Strategy"] },

                // Adventure
                new Game { Name = "Uncharted 4", Price = 39.99m, ReleaseDate = new DateOnly(2016, 5, 10), GenreID = genreMap["Adventure"] },
                new Game { Name = "Tomb Raider", Price = 19.99m, ReleaseDate = new DateOnly(2013, 3, 5), GenreID = genreMap["Adventure"] },
                new Game { Name = "The Last of Us", Price = 29.99m, ReleaseDate = new DateOnly(2013, 6, 14), GenreID = genreMap["Adventure"] }
            );

            context.SaveChanges();
          }
        })
    );
  }
}
