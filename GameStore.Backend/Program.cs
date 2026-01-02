using GameStore.Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GameStore.Backend.Services;
using GameStore.Backend.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();


// 🔹 ADD DB + SEEDING (this was missing)
builder.AddGameStoreDb();

// Built-in OpenAPI
builder.Services.AddOpenApi();

builder.Services.AddDbContext<GameStoreContext>(options => options.UseSqlite("Data Source=GameStore.db"));

var app = builder.Build();

// 🔹 APPLY MIGRATIONS AUTOMATICALLY (this was missing)
app.MigrateDb();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference(); // exposes Scalar UI
}

// app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
