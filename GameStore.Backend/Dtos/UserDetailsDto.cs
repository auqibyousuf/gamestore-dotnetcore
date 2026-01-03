using System;

namespace GameStore.Backend.Dtos;

public class UserDetailsDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Role { get; init; } = string.Empty;
}
