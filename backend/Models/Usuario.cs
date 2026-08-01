namespace backend.Models;

public class Usuario
{
    public Guid Id { get; set; }

    // Clave foránea
    public Guid TenantId { get; set; }

    // Relación con Tenant
    public Tenant Tenant { get; set; } = null!;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public Rol Rol { get; set; }

    public bool Activo { get; set; }
}