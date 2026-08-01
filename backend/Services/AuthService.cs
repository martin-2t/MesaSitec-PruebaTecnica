using backend.Data;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwtService;
    private readonly PasswordService _passwordService;

    public AuthService(
        ApplicationDbContext context,
        JwtService jwtService,
        PasswordService passwordService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u =>
                u.Email == request.Email &&
                u.Activo);

        if (usuario == null)
            return null;

        var passwordValida = _passwordService.VerifyPassword(
            usuario,
            request.Password,
            usuario.PasswordHash);

        if (!passwordValida)
            return null;

        var token = _jwtService.GenerarToken(usuario);

        return new LoginResponse
        {
            AccessToken = token,
            ExpiraEn = 28800,
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol.ToString(),
                TenantId = usuario.TenantId,
                TenantNombre = usuario.Tenant.Nombre
            }
        };
    }
}