using Domain.Enums;

namespace Application.DTOs
{
    public record SubastaDetalleDto(
        int Id,
        string Titulo,
        string Descripcion,
        string UrlImagen,
        decimal PrecioBase,
        decimal PrecioActual,
        decimal IncrementoMinimo,
        EstadoSubasta Estado,
        DateTime FechaInicio,
        DateTime FechaFin,
        int CategoriaId,
        string CategoriaNombre,
        int VendedorId,
        string VendedorNombre,
        List<PujaResumenDto> UltimasPujas
    );
    
}
