using Domain.Enums;

namespace Application.DTOs
{
    public record TransaccionLedgerDto(
        int Id,
        TipoTransaccion Tipo,
        decimal Monto,
        DateTime Fecha,
        int? SubastaId,
        string? SubastaTitulo
    );
}
