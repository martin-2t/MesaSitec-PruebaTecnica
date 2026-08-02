using backend.Models;
using backend.Services;
using Xunit;

namespace backend.Tests;

public class SlaServiceTests
{
    private readonly SlaService _slaService = new();

    [Fact]
    public void CalcularFechaLimite_Critica_CalculaLaMitadDelTiempo()
    {
        // Arrange
        var fechaCreacion = new DateTime(2026, 1, 1, 8, 0, 0);

        // Act
        var resultado = _slaService.CalcularFechaLimite(
            fechaCreacion,
            8,
            Prioridad.Critica);

        // Assert
        Assert.Equal(fechaCreacion.AddHours(4), resultado);
    }

    [Fact]
    public void CalcularFechaLimite_Baja_DuplicaElTiempo()
    {
        // Arrange
        var fechaCreacion = new DateTime(2026, 1, 1, 8, 0, 0);

        // Act
        var resultado = _slaService.CalcularFechaLimite(
            fechaCreacion,
            24,
            Prioridad.Baja);

        // Assert
        Assert.Equal(fechaCreacion.AddHours(48), resultado);
    }
}