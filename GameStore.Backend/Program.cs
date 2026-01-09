using GameStore.Backend.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GameStore.Backend.Middleware;
using GameStore.Backend.Auth;
using GameStore.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();


// 🔹 ADD DB + SEEDING (this was missing)
builder.AddGameStoreDb();

// Built-in OpenAPI
builder.Services.AddOpenApi();

builder.Services.AddDbContext<GameStoreContext>(options => options.UseSqlite("Data Source=GameStore.db"));
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateAudience = true,
    ValidateIssuer = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidAudience = builder.Configuration["Jwt:Audience"],
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))

  };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));



builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<BasketService>();
builder.Services.AddScoped<OrderService>();
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
app.UseStaticFiles();
app.Run();
