using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken ct = default);
    }
}
