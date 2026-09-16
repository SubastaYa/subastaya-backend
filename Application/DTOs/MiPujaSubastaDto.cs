using Domain.Enums;

namespace Application.DTOs
{
    public record MiPujaSubastaDto(
        int Id,
        string Titulo,
        string UrlImagen,
        EstadoSubasta Estado,
        DateTime FechaInicio,
        DateTime FechaFin,
        decimal PrecioBase,
        decimal PrecioActual,
        decimal MiPujaMaxima,
        bool EsGanador,
        bool EsLider,
        string CategoriaNombre
    );
}
