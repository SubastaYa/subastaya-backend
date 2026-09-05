namespace Application.DTOs
{
    public record PujaResumenDto(
        int Id,
        decimal Monto,
        DateTime FechaHora,
        string CompradorNombre
    );
}
