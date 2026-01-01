using System;
using System.ComponentModel.DataAnnotations;

namespace GameStore.Backend.Dtos;

public class RegisterUserDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";
    [Required, MinLength(4)]
    public string Password { get; set; } = "";
}
