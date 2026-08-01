using backend.Data;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwtService;
    private readonly PasswordService _passwordService;

    public AuthController(
        ApplicationDbContext context,
        JwtService jwtService,
        PasswordService passwordService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var usuario = await _context.Usuarios
    .Include(u => u.Tenant)
    .FirstOrDefaultAsync(u => u.Email == request.Email);

Console.WriteLine($"LOGIN -> Id: {usuario?.Id}  Email: {usuario?.Email}");

        if (usuario == null)
        {
            return Unauthorized("Email o contraseña incorrectos.");
        }

        if (!_passwordService.VerifyPassword(
            usuario,
            request.Password,
            usuario.PasswordHash))
        {
             return Unauthorized("Email o contraseña incorrectos.");
        }

        var token = _jwtService.GenerarToken(usuario);

        return Ok(new LoginResponse
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
});
    }
}