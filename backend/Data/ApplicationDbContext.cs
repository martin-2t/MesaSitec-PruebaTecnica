using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }

    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<Categoria> Categorias { get; set; }

    public DbSet<Solicitud> Solicitudes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Solicitud>()
            .HasOne(s => s.Solicitante)
            .WithMany()
            .HasForeignKey(s => s.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Solicitud>()
            .HasOne(s => s.Agente)
            .WithMany()
            .HasForeignKey(s => s.AgenteId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Usuarios)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Categoria>()
            .HasOne(c => c.Tenant)
            .WithMany(t => t.Categorias)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Solicitud>()
            .HasOne(s => s.Tenant)
            .WithMany(t => t.Solicitudes)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Solicitud>()
            .HasOne(s => s.Categoria)
            .WithMany(c => c.Solicitudes)
            .HasForeignKey(s => s.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<Categoria>()
            .Property(c => c.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Tenant>()
            .Property(t => t.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<Solicitud>()
            .Property(s => s.Titulo)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<Solicitud>()
            .Property(s => s.Descripcion)
            .HasMaxLength(4000)
            .IsRequired();

        modelBuilder.Entity<Solicitud>()
            .Property(s => s.Codigo)
            .HasMaxLength(20)
            .IsRequired();

    }
}