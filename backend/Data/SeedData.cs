using System;
using System.Linq;
using backend.Models;
using backend.Services;

namespace backend.Data;

public static class SeedData
{
    public static void Initialize(
        ApplicationDbContext context,
        PasswordService passwordService)
    {
        if (context.Tenants.Any())
        {
            return;
        }

        // ==========================
        // Tenants
        // ==========================

        var cooperativaNorte = new Tenant
        {
            Id = Guid.NewGuid(),
            Nombre = "Cooperativa Norte",
            Activo = true
        };

        var bufeteSur = new Tenant
        {
            Id = Guid.NewGuid(),
            Nombre = "Bufete Sur",
            Activo = true
        };

        context.Tenants.AddRange(cooperativaNorte, bufeteSur);
        context.SaveChanges();

        // ==========================
        // Usuarios
        // ==========================

        var adminNorte = new Usuario
        {
            Id = Guid.NewGuid(),
            TenantId = cooperativaNorte.Id,
            Email = "admin@norte.test",
            Nombre = "Administrador Norte",
            Rol = Rol.Admin,
            Activo = true
        };

        adminNorte.PasswordHash = passwordService.HashPassword(
            adminNorte,
            "Sitec.2026");

        var agente1 = new Usuario
        {
            Id = Guid.NewGuid(),
            TenantId = cooperativaNorte.Id,
            Email = "agente1@norte.test",
            Nombre = "Agente Uno",
            Rol = Rol.Agente,
            Activo = true
        };

        agente1.PasswordHash = passwordService.HashPassword(
            agente1,
            "Sitec.2026");

        var solicitante1 = new Usuario
        {
            Id = Guid.NewGuid(),
            TenantId = cooperativaNorte.Id,
            Email = "usuario1@norte.test",
            Nombre = "Usuario Uno",
            Rol = Rol.Solicitante,
            Activo = true
        };

        solicitante1.PasswordHash = passwordService.HashPassword(
            solicitante1,
            "Sitec.2026");

        context.Usuarios.AddRange(
            adminNorte,
            agente1,
            solicitante1);

        context.SaveChanges();

        // ==========================
        // Categorías
        // ==========================

        var categoriaHardware = new Categoria
        {
            Id = Guid.NewGuid(),
            TenantId = cooperativaNorte.Id,
            Nombre = "Hardware",
            SlaHoras = 24,
            Activo = true
        };

        var categoriaSoftware = new Categoria
        {
            Id = Guid.NewGuid(),
            TenantId = cooperativaNorte.Id,
            Nombre = "Software",
            SlaHoras = 48,
            Activo = true
        };

        var categoriaRedes = new Categoria
        {
            Id = Guid.NewGuid(),
            TenantId = cooperativaNorte.Id,
            Nombre = "Redes",
            SlaHoras = 12,
            Activo = true
        };

        context.Categorias.AddRange(
            categoriaHardware,
            categoriaSoftware,
            categoriaRedes);

        context.SaveChanges();

        // ==========================
        // Solicitud
        // ==========================

        var solicitud1 = new Solicitud
{
    Id = Guid.NewGuid(),
    TenantId = cooperativaNorte.Id,

    Codigo = "SOL-2026-00001",

    Titulo = "No puedo iniciar sesión",

    Descripcion = "El sistema rechaza mis credenciales.",

    CategoriaId = categoriaSoftware.Id,

    Prioridad = Prioridad.Alta,

    Estado = EstadoSolicitud.Nueva,

    SolicitanteId = solicitante1.Id,

    AgenteId = agente1.Id,

    FechaCreacion = DateTime.UtcNow,

    FechaLimiteSla = DateTime.UtcNow.AddHours(48),

    FechaResolucion = null,

    MotivoResolucion = null,

    MotivoCancelacion = null
};

context.Solicitudes.Add(solicitud1);

context.SaveChanges();
    }

    
}