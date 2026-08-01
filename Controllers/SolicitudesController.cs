using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolicitudesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SolicitudesController(ApplicationDbContext context)
    {
        _context = context;
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

        // Validación
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                codigo = "PARAMETRO_INVALIDO"
            });
        }

        var query = _context.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Agente)
            .Where(s => s.TenantId == tenantId);

        // Filtro por estado
        if (estado.HasValue)
        {
            query = query.Where(s => s.Estado == estado.Value);
        }

        // Filtro por prioridad
        if (prioridad.HasValue)
        {
            query = query.Where(s => s.Prioridad == prioridad.Value);
        }

        // Filtro por categoría
        if (categoriaId.HasValue)
        {
            query = query.Where(s => s.CategoriaId == categoriaId.Value);
        }

        // Filtro por agente
        if (agenteId.HasValue)
        {
            query = query.Where(s => s.AgenteId == agenteId.Value);
        }

        // Búsqueda
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();

            query = query.Where(s =>
                s.Codigo.Contains(q) ||
                s.Titulo.Contains(q) ||
                s.Descripcion.Contains(q));
        }

        // Solo vencidas
        if (vencidas == true)
        {
            query = query.Where(s =>
                s.FechaLimiteSla < DateTime.UtcNow &&
                s.Estado != EstadoSolicitud.Resuelta &&
                s.Estado != EstadoSolicitud.Cerrada &&
                s.Estado != EstadoSolicitud.Cancelada);
        }

        // Ordenamiento
        query = sort switch
        {
            "fechaCreacion" => query.OrderBy(s => s.FechaCreacion),

            "-fechaCreacion" or null or "" =>
                query.OrderByDescending(s => s.FechaCreacion),

            "prioridad" =>
                query.OrderBy(s => s.Prioridad),

            "-prioridad" =>
                query.OrderByDescending(s => s.Prioridad),

            "codigo" =>
                query.OrderBy(s => s.Codigo),

            _ =>
                query.OrderByDescending(s => s.FechaCreacion)
        };

        var total = await query.CountAsync();

        var solicitudes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SolicitudListResponse
            {
                Id = s.Id,
                Codigo = s.Codigo,
                Titulo = s.Titulo,
                Estado = s.Estado.ToString(),
                Prioridad = s.Prioridad.ToString(),

                CategoriaId = s.CategoriaId,
                CategoriaNombre = s.Categoria.Nombre,

                AgenteId = s.AgenteId,
                AgenteNombre = s.Agente != null ? s.Agente.Nombre : null,

                FechaCreacion = s.FechaCreacion,
                FechaLimiteSla = s.FechaLimiteSla,

                Vencida =
                    s.FechaLimiteSla < DateTime.UtcNow &&
                    s.Estado != EstadoSolicitud.Resuelta &&
                    s.Estado != EstadoSolicitud.Cerrada &&
                    s.Estado != EstadoSolicitud.Cancelada
            })
            .ToListAsync();

        var respuesta = new SolicitudPaginadaResponse
        {
            Items = solicitudes,
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)pageSize)
        };

        return Ok(respuesta);
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

        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c =>
                c.Id == request.CategoriaId &&
                c.TenantId == tenantId &&
                c.Activo);

        if (categoria == null)
        {
            return BadRequest(new
            {
                codigo = "CATEGORIA_INVALIDA"
            });
        }

        var ultimoCodigo = await _context.Solicitudes
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.Codigo)
            .Select(s => s.Codigo)
            .FirstOrDefaultAsync();

        int numero = 1;

        if (!string.IsNullOrWhiteSpace(ultimoCodigo))
        {
            var partes = ultimoCodigo.Split('-');

            if (partes.Length == 3 &&
                int.TryParse(partes[2], out var ultimoNumero))
            {
                numero = ultimoNumero + 1;
            }
        }

        var codigo = $"SOL-{DateTime.UtcNow.Year}-{numero:D5}";

        var fechaCreacion = DateTime.UtcNow;

        var solicitud = new Solicitud
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = codigo,
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            CategoriaId = categoria.Id,
            Prioridad = request.Prioridad,
            Estado = EstadoSolicitud.Nueva,
            SolicitanteId = solicitanteId,
            AgenteId = null,
            FechaCreacion = fechaCreacion,
            FechaLimiteSla = fechaCreacion.AddHours(categoria.SlaHoras),
            FechaResolucion = null,
            MotivoResolucion = null,
            MotivoCancelacion = null
        };

        _context.Solicitudes.Add(solicitud);
        await _context.SaveChangesAsync();

        var respuesta = new SolicitudDetailResponse
        {
            Id = solicitud.Id,
            Codigo = solicitud.Codigo,
            Titulo = solicitud.Titulo,
            Descripcion = solicitud.Descripcion,
            Estado = solicitud.Estado.ToString(),
            Prioridad = solicitud.Prioridad.ToString(),

            CategoriaId = categoria.Id,
            CategoriaNombre = categoria.Nombre,

            SolicitanteId = solicitanteId,
            SolicitanteNombre = User.FindFirstValue("email") ?? "",

            AgenteId = null,
            AgenteNombre = null,

            FechaCreacion = solicitud.FechaCreacion,
            FechaLimiteSla = solicitud.FechaLimiteSla,

            FechaResolucion = null,
            MotivoResolucion = null,
            MotivoCancelacion = null
        };

        return CreatedAtAction(
            nameof(CreateSolicitud),
            new { id = solicitud.Id },
            respuesta);
    }
}