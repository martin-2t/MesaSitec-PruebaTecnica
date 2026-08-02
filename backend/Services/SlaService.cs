using backend.Models;

namespace backend.Services;

public class SlaService
{
    public DateTime CalcularFechaLimite(
        DateTime fechaCreacion,
        int slaHoras,
        Prioridad prioridad)
    {
        var factor = prioridad switch
        {
            Prioridad.Critica => 0.5,
            Prioridad.Alta => 0.75,
            Prioridad.Media => 1.0,
            Prioridad.Baja => 2.0,

            _ => throw new ArgumentOutOfRangeException(
                nameof(prioridad),
                prioridad,
                "Prioridad no válida.")
        };

        return fechaCreacion.AddHours(slaHoras * factor);
    }
}