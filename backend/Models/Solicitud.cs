namespace backend.Models;

public class Solicitud

{
    public Guid Id { get; set; }

    // Tenant
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    // Categoría
    public Guid CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public Prioridad Prioridad { get; set; }

    public EstadoSolicitud Estado { get; set; }

    // Usuario que creó la solicitud
    public Guid SolicitanteId { get; set; }
    public Usuario Solicitante { get; set; } = null!;

    // Usuario asignado (puede ser null)
    public Guid? AgenteId { get; set; }
    public Usuario? Agente { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaLimiteSla { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public string? MotivoResolucion { get; set; }

    public string? MotivoCancelacion { get; set; }
}