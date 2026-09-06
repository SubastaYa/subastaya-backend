using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ISubastaRepository
    {
        Task<IReadOnlyList<SubastaListDto>> ObtenerCatalogoAsync(int? categoriaId, string? estado, string? busqueda, CancellationToken ct = default);
        Task<SubastaDetalleDto?> ObtenerDetallePorIdAsync(int id, CancellationToken ct = default);
        Task<Subasta?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        Task<Subasta?> ObtenerConPujasPorIdAsync(int id, CancellationToken ct = default);
        Task AgregarAsync(Subasta subasta, CancellationToken ct = default);
    }
}
