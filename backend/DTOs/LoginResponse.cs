namespace backend.DTOs;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public int ExpiraEn { get; set; }

    public UsuarioDto Usuario { get; set; } = new();
}

public class UsuarioDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public Guid TenantId { get; set; }

    public string TenantNombre { get; set; } = string.Empty;
}