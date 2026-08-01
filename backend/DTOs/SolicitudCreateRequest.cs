using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class SolicitudCreateRequest
{
    [Required]
    [MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public Guid CategoriaId { get; set; }

    [Required]
    public Prioridad Prioridad { get; set; }
}