using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CategoriasController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize]
[HttpGet]
public async Task<IActionResult> GetCategorias()
{
    var tenantIdClaim = User.FindFirstValue("tenantId");

    if (tenantIdClaim == null)
        return Unauthorized();

    if (!Guid.TryParse(tenantIdClaim, out var tenantId))
        return Unauthorized();

    var categorias = await _context.Categorias
        .Where(c => c.Activo && c.TenantId == tenantId)
        .Select(c => new
        {
            id = c.Id,
            nombre = c.Nombre,
            slaHoras = c.SlaHoras
        })
        .ToListAsync();

    return Ok(categorias);
}
}