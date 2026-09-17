using Domain.Enums;

namespace Application.DTOs
{
    public record MiOfertaSubastaDto(
        int Id,
        string Titulo,
        string UrlImagen,
        EstadoSubasta Estado,
        DateTime FechaInicio,
        DateTime FechaFin,
        decimal PrecioBase,
        decimal PrecioActual,
        decimal MiOfertaMaxima,
        bool EsGanador,
        bool EsLider,
        string CategoriaNombre
    );
}
