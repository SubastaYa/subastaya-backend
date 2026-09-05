using Domain.Enums;

namespace Application.DTOs
{
    public record SubastaListDto(
        int Id,
        string Titulo,
        string UrlImagen,
        decimal PrecioBase,
        decimal PrecioActual,
        EstadoSubasta Estado,
        DateTime FechaFin,
        string CategoriaNombre,
        string VendedorNombre,
        int TotalPujas
    );
}
