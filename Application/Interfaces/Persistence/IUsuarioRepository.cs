using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct = default);
        Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    }
}
