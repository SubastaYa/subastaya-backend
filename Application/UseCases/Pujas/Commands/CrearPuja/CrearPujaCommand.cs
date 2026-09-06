using Application.Interfaces;

namespace Application.UseCases.Pujas.Commands.CrearPuja
{
    public record CrearPujaCommand(int SubastaId, int CompradorId, decimal Monto) : ICommand<int>;
}
