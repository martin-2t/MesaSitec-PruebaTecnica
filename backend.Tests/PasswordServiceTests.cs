using backend.Models;
using backend.Services;
using Xunit;

namespace backend.Tests;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService = new();

    [Fact]
    public void HashPassword_GeneraUnHashDistintoAlTextoPlano()
    {
        // Arrange
        var usuario = new Usuario();
        var password = "MiPassword123!";

        // Act
        var hash = _passwordService.HashPassword(usuario, password);

        // Assert
        Assert.NotEqual(password, hash);
        Assert.False(string.IsNullOrWhiteSpace(hash));
    }

    [Fact]
    public void VerifyPassword_ConPasswordCorrecta_DevuelveTrue()
    {
        // Arrange
        var usuario = new Usuario();
        var password = "MiPassword123!";

        var hash = _passwordService.HashPassword(usuario, password);

        // Act
        var resultado = _passwordService.VerifyPassword(usuario, password, hash);

        // Assert
        Assert.True(resultado);
    }
}