using System;
using System.ComponentModel.DataAnnotations;

namespace GameStore.Backend.Dtos;

public class LoginUserDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}
