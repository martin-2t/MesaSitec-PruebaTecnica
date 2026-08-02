using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolicitudesController : ControllerBase
{
    private readonly SolicitudService _solicitudService;

    public SolicitudesController(SolicitudService solicitudService)
    {
        _solicitudService = solicitudService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetSolicitudes(
        [FromQuery] EstadoSolicitud? estado,
        [FromQuery] Prioridad? prioridad,
        [FromQuery] Guid? categoriaId,
        [FromQuery] Guid? agenteId,
        [FromQuery] string? q,
        [FromQuery] bool? vencidas,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var tenantIdClaim = User.FindFirstValue("tenantId");

        if (tenantIdClaim == null)
            return Unauthorized();

        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                codigo = "PARAMETRO_INVALIDO"
            });
        }

        var resultado = await _solicitudService.GetSolicitudes(
            tenantId,
            estado,
            prioridad,
            categoriaId,
            agenteId,
            q,
            vencidas,
            sort,
            page,
            pageSize);

        return Ok(resultado);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateSolicitud(
        [FromBody] SolicitudCreateRequest request)
    {
        var tenantIdClaim = User.FindFirstValue("tenantId");
        var userIdClaim = User.FindFirstValue("sub");

        if (tenantIdClaim == null || userIdClaim == null)
            return Unauthorized();

        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        if (!Guid.TryParse(userIdClaim, out var solicitanteId))
            return Unauthorized();

        var resultado = await _solicitudService.CreateSolicitud(
            tenantId,
            solicitanteId,
            request);

        if (resultado == null)
        {
            return BadRequest(new
            {
                codigo = "CATEGORIA_INVALIDA"
            });
        }

        return CreatedAtAction(
            nameof(GetSolicitud),
            new { id = resultado.Id },
            resultado);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSolicitud(Guid id)
    {
        var tenantIdClaim = User.FindFirstValue("tenantId");

        if (tenantIdClaim == null)
            return Unauthorized();

        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var resultado = await _solicitudService.GetSolicitud(
            id,
            tenantId);

        if (resultado == null)
            return NotFound();

        return Ok(resultado);
    }
    [Authorize]
[HttpPut("{id}")]
public async Task<IActionResult> UpdateSolicitud(
    Guid id,
    [FromBody] SolicitudUpdateRequest request)
{
    var tenantIdClaim = User.FindFirstValue("tenantId");
    var userIdClaim = User.FindFirstValue("sub");
    var rol = User.FindFirstValue("rol");

    if (tenantIdClaim == null ||
        userIdClaim == null ||
        rol == null)
    {
        return Unauthorized();
    }

    if (!Guid.TryParse(tenantIdClaim, out var tenantId) ||
        !Guid.TryParse(userIdClaim, out var userId))
    {
        return Unauthorized();
    }

    var resultado = await _solicitudService.UpdateSolicitud(
        id,
        tenantId,
        userId,
        rol,
        request);

    if (resultado == null)
    {
        return NotFound();
    }

    return Ok(resultado);
}
 [Authorize]
[HttpPost("{id}/transiciones")]
public async Task<IActionResult> EjecutarTransicion(
    Guid id,
    [FromBody] SolicitudTransicionRequest request)
{
    var tenantIdClaim = User.FindFirstValue("tenantId");
    var userIdClaim = User.FindFirstValue("sub");
    var rol = User.FindFirstValue("rol");

    if (tenantIdClaim == null ||
        userIdClaim == null ||
        rol == null)
        return Unauthorized();

    if (!Guid.TryParse(tenantIdClaim, out var tenantId) ||
        !Guid.TryParse(userIdClaim, out var userId))
        return Unauthorized();

    var resultado =
        await _solicitudService.EjecutarTransicion(
            id,
            tenantId,
            userId,
            rol,
            request);

    return Ok(resultado);
}
}