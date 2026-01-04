using System;

namespace GameStore.Backend.Dtos;

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
