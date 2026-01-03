using System;
using System.ComponentModel.DataAnnotations;

namespace GameStore.Backend.Dtos;

public class RegisterUserDto
{
    [Required]
    public string Name { get; set; } = "";
    [Required, EmailAddress]
    public string Email { get; set; } = "";
    [Required, MinLength(4)]
    public string Password { get; set; } = "";
}
