namespace GameStore.Backend.Dtos.Common;

public class UserContextDto
{
    public int UserId { get; set; }
    public string Role { get; set; } = null!;
    public string Email { get; set; } = null!;
}
