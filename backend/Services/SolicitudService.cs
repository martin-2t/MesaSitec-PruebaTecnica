using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class SolicitudService
{
    private readonly ApplicationDbContext _context;

    public SolicitudService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SolicitudPaginadaResponse> GetSolicitudes(
        Guid tenantId,
        EstadoSolicitud? estado,
        Prioridad? prioridad,
        Guid? categoriaId,
        Guid? agenteId,
        string? q,
        bool? vencidas,
        string? sort,
        int page,
        int pageSize)
    {
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
            "fechaCreacion" =>
                query.OrderBy(s => s.FechaCreacion),

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
                AgenteNombre = s.Agente != null
                    ? s.Agente.Nombre
                    : null,

                FechaCreacion = s.FechaCreacion,
                FechaLimiteSla = s.FechaLimiteSla,

                Vencida =
                    s.FechaLimiteSla < DateTime.UtcNow &&
                    s.Estado != EstadoSolicitud.Resuelta &&
                    s.Estado != EstadoSolicitud.Cerrada &&
                    s.Estado != EstadoSolicitud.Cancelada
            })
            .ToListAsync();

        return new SolicitudPaginadaResponse
        {
            Items = solicitudes,
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPaginas =
                (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<SolicitudDetailResponse?> CreateSolicitud(
        Guid tenantId,
        Guid solicitanteId,
        SolicitudCreateRequest request)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c =>
                c.Id == request.CategoriaId &&
                c.TenantId == tenantId &&
                c.Activo);

        if (categoria == null)
            return null;

        // Buscar el último código de este tenant
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

            FechaLimiteSla =
                fechaCreacion.AddHours(categoria.SlaHoras),

            FechaResolucion = null,
            MotivoResolucion = null,
            MotivoCancelacion = null
        };

        _context.Solicitudes.Add(solicitud);

        await _context.SaveChangesAsync();

        var solicitante = await _context.Usuarios
            .FirstOrDefaultAsync(u =>
                u.Id == solicitanteId &&
                u.TenantId == tenantId);

        return new SolicitudDetailResponse
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

            SolicitanteNombre = solicitante?.Nombre ?? "",

            AgenteId = null,

            AgenteNombre = null,

            FechaCreacion = solicitud.FechaCreacion,

            FechaLimiteSla = solicitud.FechaLimiteSla,

            FechaResolucion = null,

            MotivoResolucion = null,

            MotivoCancelacion = null
        };
    }

    public async Task<SolicitudDetailResponse?> GetSolicitud(
        Guid id,
        Guid tenantId)
    {
        var solicitud = await _context.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Solicitante)
            .Include(s => s.Agente)
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.TenantId == tenantId);

        if (solicitud == null)
            return null;

        return new SolicitudDetailResponse
        {
            Id = solicitud.Id,

            Codigo = solicitud.Codigo,

            Titulo = solicitud.Titulo,

            Descripcion = solicitud.Descripcion,

            Estado = solicitud.Estado.ToString(),

            Prioridad = solicitud.Prioridad.ToString(),

            CategoriaId = solicitud.CategoriaId,

            CategoriaNombre = solicitud.Categoria.Nombre,

            SolicitanteId = solicitud.SolicitanteId,

            SolicitanteNombre = solicitud.Solicitante.Nombre,

            AgenteId = solicitud.AgenteId,

            AgenteNombre = solicitud.Agente != null
                ? solicitud.Agente.Nombre
                : null,

            FechaCreacion = solicitud.FechaCreacion,

            FechaLimiteSla = solicitud.FechaLimiteSla,

            FechaResolucion = solicitud.FechaResolucion,

            MotivoResolucion = solicitud.MotivoResolucion,

            MotivoCancelacion = solicitud.MotivoCancelacion
        };
    }
}