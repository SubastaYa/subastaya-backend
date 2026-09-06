using Domain.Entities;

namespace Application.Interfaces
{
    public interface IPujaRepository
    {
        Task AgregarAsync(Puja puja, CancellationToken ct = default);
    }
}
