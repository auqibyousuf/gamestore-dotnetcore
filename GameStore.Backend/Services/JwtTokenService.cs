using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameStore.Backend.Models;
using Microsoft.IdentityModel.Tokens;

namespace GameStore.Backend.Services;

public class JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<JwtTokenService> _logger = logger;

    public string GenerateToken(User user)
    {

        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };



        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(

            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.UtcNow.AddMonths(3),
            claims: claims,
            signingCredentials: creds

        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
