
namespace Application.UseCases.Ofertas.CrearOferta
{
    public record CrearOfertaCommand(int SubastaId, int CompradorId, decimal Monto) : ICommand<int>;
}
