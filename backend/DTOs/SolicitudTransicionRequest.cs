using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class SolicitudTransicionRequest
{
    [Required]
    public string Accion { get; set; } = string.Empty;

    public Guid? AgenteId { get; set; }

    public string? Motivo { get; set; }
}