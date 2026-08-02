using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class SolicitudService
{
    private readonly ApplicationDbContext _context;
private readonly SlaService _slaService;

public SolicitudService(
    ApplicationDbContext context,
    SlaService slaService)
{
    _context = context;
    _slaService = slaService;
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
        var ahora = DateTime.UtcNow;

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

        // Búsqueda por código, título o descripción
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();

            query = query.Where(s =>
                s.Codigo.ToLower().Contains(q.ToLower()) ||
                s.Titulo.ToLower().Contains(q.ToLower()) ||
                s.Descripcion.ToLower().Contains(q.ToLower()));
        }

        // Solo solicitudes vencidas
        if (vencidas == true)
        {
            query = query.Where(s =>
                s.FechaLimiteSla < ahora &&
                s.Estado != EstadoSolicitud.Resuelta &&
                s.Estado != EstadoSolicitud.Cerrada &&
                s.Estado != EstadoSolicitud.Cancelada);
        }

        // Ordenamiento
        //
        // Prioridad:
        // Critica > Alta > Media > Baja
        //
        query = sort switch
        {
            "fechaCreacion" =>
                query.OrderBy(s => s.FechaCreacion),

            "-fechaCreacion" or null or "" =>
                query.OrderByDescending(s => s.FechaCreacion),

            "prioridad" =>
                query.OrderByDescending(s =>
                    s.Prioridad == Prioridad.Critica ? 4 :
                    s.Prioridad == Prioridad.Alta ? 3 :
                    s.Prioridad == Prioridad.Media ? 2 : 1),

            "-prioridad" =>
                query.OrderBy(s =>
                    s.Prioridad == Prioridad.Critica ? 4 :
                    s.Prioridad == Prioridad.Alta ? 3 :
                    s.Prioridad == Prioridad.Media ? 2 : 1),

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
                    s.FechaLimiteSla < ahora &&
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

        var fechaCreacion = DateTime.UtcNow;

        // El código es independiente por tenant y por año.
        var año = fechaCreacion.Year;

        var ultimoCodigo = await _context.Solicitudes
            .Where(s =>
                s.TenantId == tenantId &&
                s.Codigo.StartsWith($"SOL-{año}-"))
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

        var codigo = $"SOL-{año}-{numero:D5}";

        var fechaLimiteSla = _slaService.CalcularFechaLimite(
    fechaCreacion,
    categoria.SlaHoras,
    request.Prioridad);

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

            FechaLimiteSla = fechaLimiteSla,

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


    public async Task<SolicitudDetailResponse?> UpdateSolicitud(
    Guid id,
    Guid tenantId,
    Guid userId,
    string rol,
    SolicitudUpdateRequest request)
{
    var solicitud = await _context.Solicitudes
        .Include(s => s.Categoria)
        .Include(s => s.Solicitante)
        .Include(s => s.Agente)
        .FirstOrDefaultAsync(s =>
            s.Id == id &&
            s.TenantId == tenantId);

    // RN-01: si no existe o pertenece a otro tenant, 404
    if (solicitud == null)
        return null;

    // RN-03: verificar permisos
    var esAdminOAgente =
        rol == "Admin" ||
        rol == "Agente";

    var esSolicitantePropio =
        rol == "Solicitante" &&
        solicitud.SolicitanteId == userId;

    if (!esAdminOAgente && !esSolicitantePropio)
        throw new UnauthorizedAccessException("OPERACION_NO_PERMITIDA");

    // Un Solicitante solo puede editar solicitudes en estado Nueva.
    if (rol == "Solicitante" &&
        solicitud.Estado != EstadoSolicitud.Nueva)
    {
        throw new UnauthorizedAccessException("OPERACION_NO_PERMITIDA");
    }

    // La categoría debe existir, estar activa y pertenecer al tenant.
    var categoria = await _context.Categorias
        .FirstOrDefaultAsync(c =>
            c.Id == request.CategoriaId &&
            c.TenantId == tenantId &&
            c.Activo);

    if (categoria == null)
        throw new ArgumentException("CATEGORIA_INVALIDA");

    // Detectamos si cambió alguno de los valores que afectan al SLA.
    var cambioCategoria =
        solicitud.CategoriaId != request.CategoriaId;

    var cambioPrioridad =
        solicitud.Prioridad != request.Prioridad;

    // Actualizamos los datos permitidos.
    solicitud.Titulo = request.Titulo;
    solicitud.Descripcion = request.Descripcion;
    solicitud.CategoriaId = request.CategoriaId;
    solicitud.Prioridad = request.Prioridad;

    // RN-04:
    // Si cambia categoría o prioridad y la solicitud todavía
    // no está resuelta, se recalcula el SLA.
    if ((cambioCategoria || cambioPrioridad) &&
        solicitud.Estado != EstadoSolicitud.Resuelta &&
        solicitud.Estado != EstadoSolicitud.Cerrada &&
        solicitud.Estado != EstadoSolicitud.Cancelada)
    {
        solicitud.FechaLimiteSla = _slaService.CalcularFechaLimite(
    solicitud.FechaCreacion,
    categoria.SlaHoras,
    request.Prioridad);
    }

    await _context.SaveChangesAsync();

    return new SolicitudDetailResponse
    {
        Id = solicitud.Id,

        Codigo = solicitud.Codigo,

        Titulo = solicitud.Titulo,

        Descripcion = solicitud.Descripcion,

        Estado = solicitud.Estado.ToString(),

        Prioridad = solicitud.Prioridad.ToString(),

        CategoriaId = solicitud.CategoriaId,

        CategoriaNombre = categoria.Nombre,

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


    
    public async Task<SolicitudDetailResponse?> EjecutarTransicion(
    Guid id,
    Guid tenantId,
    Guid userId,
    string rol,
    SolicitudTransicionRequest request)
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

    request.Accion = request.Accion.Trim().ToLower();
switch (request.Accion)
{
    case "asignar":
        await AsignarSolicitud(solicitud, tenantId, rol, request);
        break;

    case "iniciar":
        IniciarSolicitud(solicitud, rol);
        break;

    case "resolver":
        ResolverSolicitud(solicitud, rol, request);
        break;
    
    case "cerrar":
    CerrarSolicitud(solicitud, rol);
    break;
    
    case "reabrir":
    ReabrirSolicitud(solicitud, rol);
    break;

case "cancelar":
    CancelarSolicitud(solicitud, rol, request);
    break;

    default:
        throw new InvalidOperationException("TRANSICION_INVALIDA");
}

    await _context.SaveChangesAsync();

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
        AgenteNombre = solicitud.Agente?.Nombre,
        FechaCreacion = solicitud.FechaCreacion,
        FechaLimiteSla = solicitud.FechaLimiteSla,
        FechaResolucion = solicitud.FechaResolucion,
        MotivoResolucion = solicitud.MotivoResolucion,
        MotivoCancelacion = solicitud.MotivoCancelacion
    };
}

private async Task AsignarSolicitud(
    Solicitud solicitud,
    Guid tenantId,
    string rol,
    SolicitudTransicionRequest request)
{
    // RN-03
    if (rol != "Admin" && rol != "Agente")
        throw new UnauthorizedAccessException("OPERACION_NO_PERMITIDA");

    // RN-02
    if (solicitud.Estado != EstadoSolicitud.Nueva &&
        solicitud.Estado != EstadoSolicitud.Asignada &&
        solicitud.Estado != EstadoSolicitud.EnProceso)
    {
        throw new InvalidOperationException("TRANSICION_INVALIDA");
    }

    if (request.AgenteId == null)
        throw new ArgumentException("AGENTE_INVALIDO");

    // RN-05
    var agente = await _context.Usuarios
        .FirstOrDefaultAsync(u =>
            u.Id == request.AgenteId &&
            u.TenantId == tenantId &&
            u.Activo &&
            (u.Rol == Rol.Admin || u.Rol == Rol.Agente));

    if (agente == null)
        throw new ArgumentException("AGENTE_INVALIDO");

    solicitud.AgenteId = agente.Id;
    solicitud.Agente = agente;
    solicitud.Estado = EstadoSolicitud.Asignada;
}

private void IniciarSolicitud(
    Solicitud solicitud,
    string rol)
{
    // RN-03
    if (rol != "Admin" && rol != "Agente")
        throw new UnauthorizedAccessException("OPERACION_NO_PERMITIDA");

    // RN-02
    if (solicitud.Estado != EstadoSolicitud.Asignada)
        throw new InvalidOperationException("TRANSICION_INVALIDA");

    solicitud.Estado = EstadoSolicitud.EnProceso;
}
private void ResolverSolicitud(
    Solicitud solicitud,
    string rol,
    SolicitudTransicionRequest request)
{
    // RN-03
    if (rol != "Admin" && rol != "Agente")
        throw new UnauthorizedAccessException("OPERACION_NO_PERMITIDA");

    // RN-02
    if (solicitud.Estado != EstadoSolicitud.EnProceso)
        throw new InvalidOperationException("TRANSICION_INVALIDA");

    // RN-06
    if (string.IsNullOrWhiteSpace(request.Motivo) ||
        request.Motivo.Trim().Length < 20)
    {
        throw new ArgumentException("MOTIVO_REQUERIDO");
    }

    solicitud.Estado = EstadoSolicitud.Resuelta;
    solicitud.MotivoResolucion = request.Motivo.Trim();
    solicitud.FechaResolucion = DateTime.UtcNow;
}

private void CerrarSolicitud(
    Solicitud solicitud,
    string rol)
{
    // RN-03
    if (rol != "Admin" && rol != "Agente")
        throw new UnauthorizedAccessException("OPERACION_NO_PERMITIDA");

    // RN-02
    if (solicitud.Estado != EstadoSolicitud.Resuelta)
        throw new InvalidOperationException("TRANSICION_INVALIDA");

    solicitud.Estado = 
    EstadoSolicitud.Cerrada;
}

private void ReabrirSolicitud(
    Solicitud solicitud,
    string rol)
{
    // RN-03
    if (rol != "Admin" && rol != "Agente")
        throw new UnauthorizedAccessException("OPERACION_NO_PERMITIDA");

    // RN-02
    if (solicitud.Estado != EstadoSolicitud.Resuelta &&
        solicitud.Estado != EstadoSolicitud.Cerrada)
    {
        throw new InvalidOperationException("TRANSICION_INVALIDA");
    }

    solicitud.Estado = EstadoSolicitud.EnProceso;
}

private void CancelarSolicitud(
    Solicitud solicitud,
    string rol,
    SolicitudTransicionRequest request)
{
    // RN-03
    if (rol != "Admin" && rol != "Agente")
        throw new UnauthorizedAccessException("OPERACION_NO_PERMITIDA");

    // RN-02
    if (solicitud.Estado != EstadoSolicitud.Nueva &&
        solicitud.Estado != EstadoSolicitud.Asignada &&
        solicitud.Estado != EstadoSolicitud.EnProceso)
    {
        throw new InvalidOperationException("TRANSICION_INVALIDA");
    }

    // RN-06
    if (string.IsNullOrWhiteSpace(request.Motivo) ||
    request.Motivo.Trim().Length < 10)
{
    throw new ArgumentException("MOTIVO_REQUERIDO");
}

    solicitud.Estado = EstadoSolicitud.Cancelada;
    solicitud.MotivoCancelacion = request.Motivo.Trim();
}
}