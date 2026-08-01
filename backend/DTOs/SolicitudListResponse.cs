namespace backend.DTOs;

public class SolicitudListResponse
{
    public Guid Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public string Prioridad { get; set; } = string.Empty;

    public Guid CategoriaId { get; set; }

    public string CategoriaNombre { get; set; } = string.Empty;

    public Guid? AgenteId { get; set; }

    public string? AgenteNombre { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaLimiteSla { get; set; }

    public bool Vencida { get; set; }
}