namespace backend.Models;

public class Categoria
{
    public Guid Id { get; set; }

    // Clave foránea
    public Guid TenantId { get; set; }

    // Relación con Tenant
    public Tenant Tenant { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;

    public int SlaHoras { get; set; }

    public bool Activo { get; set; }

    // Una categoría puede tener muchas solicitudes
    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
}