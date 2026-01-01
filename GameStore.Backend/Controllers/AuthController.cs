using GameStore.Backend.Data;
using GameStore.Backend.Dtos;
using GameStore.Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(GameStoreContext context) : ControllerBase
    {
        private readonly GameStoreContext _context = context;
        private readonly PasswordHasher<User> _passwordHasher = new();


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

            //Verify Pass
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid Email or Password");

            return Ok(new
            {
                user.Id,
                user.Email,
                user.Role
            });
        }

    }
}
