using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class SolicitudUpdateRequest
{
    [Required]
    [StringLength(120, MinimumLength = 5)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 10)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public Guid CategoriaId { get; set; }

    [Required]
    public Prioridad Prioridad { get; set; }
}