using Domain.Enums;

namespace Application.DTOs
{
    public record MiPublicacionDto(
        int Id,
        string Titulo,
        string UrlImagen,
        decimal PrecioBase,
        decimal PrecioActual,
        EstadoSubasta Estado,
        DateTime FechaInicio,
        DateTime FechaFin,
        string CategoriaNombre,
        int TotalOfertas,
        decimal MontoRecaudado,
        string? GanadorNombreOfuscado
    )
    ;
}
