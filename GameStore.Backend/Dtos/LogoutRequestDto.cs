using System;

namespace GameStore.Backend.Dtos;

public class LogoutRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
