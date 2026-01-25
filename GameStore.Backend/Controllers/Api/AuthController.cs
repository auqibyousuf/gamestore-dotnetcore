using System.Security.Cryptography;
using GameStore.Backend.Auth;
using GameStore.Backend.Data;
using GameStore.Backend.Dtos;
using GameStore.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(GameStoreContext context, JwtTokenService jwtTokenService) : ControllerBase
    {
        private readonly JwtTokenService _jwtTokenService = jwtTokenService;
        private readonly GameStoreContext _context = context;
        private readonly PasswordHasher<User> _passwordHasher = new();
        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        //Register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            var userExists = await _context.Users.AnyAsync(user => user.Email == dto.Email);
            if (userExists)
                return BadRequest("EmailID already registered");

            //Create user
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Role = "User"
            };

            //Hash Pass
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            //Save User
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role
            });
        }


        //Login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Email == dto.Email);
            if (user is null)
                return Unauthorized("Invalid Email or Password");
            var hasher = new PasswordHasher<User>();

            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid Credentials");

            var accessToken = jwtTokenService.GenerateToken(user);

            var refreshToken = new RefreshToken
            {
                Token = GenerateRefreshToken(),
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken,
                refreshToken = refreshToken.Token
            });
        }

        //-----Refresh Token
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto dto)
        {
            var storedToken = await _context.RefreshTokens.Include(rt => rt.User).FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);
            if (storedToken == null)
                return Unauthorized("Invalid token");
            if (storedToken.IsRevoked)
                return Unauthorized("Token revoked");
            if (storedToken.ExpiresAt < DateTime.UtcNow)
                return Unauthorized("Refresh token expired");

            var newAccessToken = _jwtTokenService.GenerateToken(storedToken.User);
            return Ok(new
            {
                accessToken = newAccessToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutRequestDto dto)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);
            if (token == null)
                return BadRequest("Invalid refresh token");
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Logged out successfully"
            });
        }

    }
}
