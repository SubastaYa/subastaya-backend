namespace Application.DTOs
{
    public record CrearSubastaDto(
        string Titulo,
        string Descripcion,
        string UrlImagen,
        decimal PrecioBase,
        decimal IncrementoMinimo,
        DateTime FechaInicio,
        DateTime FechaFin,
        int CategoriaId
    );
}
