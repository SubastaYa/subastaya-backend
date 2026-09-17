namespace Application.DTOs
{
    public record OfertaResumenDto(
        int Id,
        decimal Monto,
        DateTime FechaHora,
        string CompradorNombre
    );
}
