using Application.Interfaces;

namespace Application.UseCases.Pujas.CrearPuja
{
    public record CrearPujaCommand(int SubastaId, int CompradorId, decimal Monto) : ICommand<int>;
}
