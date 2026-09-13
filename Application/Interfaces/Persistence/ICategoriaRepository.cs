using Application.DTOs;
namespace Application.Interfaces.Persistence
{
    public interface ICategoriaRepository
    {
        Task<IReadOnlyList<CategoriaDto>> ObtenerTodasAsync(CancellationToken ct = default);
    }
}
