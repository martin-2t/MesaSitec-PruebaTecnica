namespace backend.DTOs;

public class SolicitudDetailResponse
{
    public Guid Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public string Prioridad { get; set; } = string.Empty;

    public Guid CategoriaId { get; set; }

    public string CategoriaNombre { get; set; } = string.Empty;

    public Guid SolicitanteId { get; set; }

    public string SolicitanteNombre { get; set; } = string.Empty;

    public Guid? AgenteId { get; set; }

    public string? AgenteNombre { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaLimiteSla { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public string? MotivoResolucion { get; set; }

    public string? MotivoCancelacion { get; set; }
}