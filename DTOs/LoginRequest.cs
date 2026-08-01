namespace backend.DTOs;

/// <summary>
/// Datos enviados por el usuario para iniciar sesión.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}