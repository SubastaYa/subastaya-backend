using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IPujaRepository
    {
        Task AgregarAsync(Puja puja, CancellationToken ct = default);
    }
}
