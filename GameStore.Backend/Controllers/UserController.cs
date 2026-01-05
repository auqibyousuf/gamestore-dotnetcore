using System.Security.Claims;
using GameStore.Backend.Data;
using GameStore.Backend.Dtos;
using GameStore.Backend.Dtos.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Backend.Controllers
{
    [ApiController]
    [Route("/api/users")]
    public class UserController(GameStoreContext context) : ControllerBase
    {
        private readonly GameStoreContext _context = context;

        [HttpGet]
        public async Task<ActionResult<List<UserListDto>>> GetUsers()
        {
            var users = await _context.Users.AsNoTracking().Select(user => new UserListDto
            {
                Name = user.Name,
                Email = user.Email,
                Id = user.Id
            }).ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailsDto>> GetUserDetails(int id)
        {
            var user = await _context.Users.Where(u => u.Id == id).Select(user => new UserDetailsDto
            {
                Name = user.Name,
                Email = user.Email,
                Id = user.Id,
                Role = user.Role
            }).FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(BaseResponse<UserDetailsDto>.Ok(user, "User fetched Successfully"));
        }

        //--------User Profile--------
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult> MyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var user = await _context.Users.AsNoTracking().Where(u => u.Id == int.Parse(userId)).Select(u => new UserProfileDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            }).FirstOrDefaultAsync();

            if (user is null)
                return NotFound();

            return Ok(user);
        }

        [Authorize]
        [HttpPut("/me")]
        public async Task<ActionResult<UpdateProfileDto>> UpdateProfile(UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();
            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user is null)
                return NotFound();

            user.Name = dto.Name;
            user.Email = dto.Email;
            await _context.SaveChangesAsync();
            return Ok(BaseResponse<object>.Ok(new
            {
                user.Id,
                user.Email,
                user.Name
            }, "Profile Updated Successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/roles")]
        public async Task<IActionResult> ChangeUserRole(int id, ChangeRoleDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
                return NotFound(BaseResponse<object>.Fail("User not found"));
            user.Role = dto.Role;
            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new { user.Id, user.Email }, $"Role Changed Successfully to {user.Role}"));
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleteUser = await _context.Users.FindAsync(id);
            if (deleteUser == null)
                return NotFound();

            _context.Users.Remove(deleteUser);
            await _context.SaveChangesAsync();
            return Ok($"User Deleted Successfully: {deleteUser.Name}");
        }
    }
}
