using System;

namespace GameStore.Backend.Settings;

public class RazorSettings
{
    public string KeyId { get; set; } = null!;
    public string KeySecret { get; set; } = null!;
    public string Currency { get; set; } = "INR";
}
