using backend.Models;
using Microsoft.AspNetCore.Identity;

namespace backend.Services;

/// <summary>
/// Servicio encargado de generar y verificar hashes de contraseñas.
/// </summary>
public class PasswordService
{
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public string HashPassword(Usuario usuario, string password)
    {
        return _passwordHasher.HashPassword(usuario, password);
    }

    public bool VerifyPassword(Usuario usuario, string password, string hash)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            usuario,
            hash,
            password);

        return result == PasswordVerificationResult.Success;
    }
}