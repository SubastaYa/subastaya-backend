using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface ISubastaRepository
    {
        Task<IReadOnlyList<SubastaListDto>> ObtenerCatalogoAsync(
            int? categoriaId,
            string? estado,
            string? busqueda,
            decimal? precioMin = null,
            decimal? precioMax = null,
            string? orden = null,
            int? page = null,
            int? pageSize = null,
            int? vendedorId = null,
            int? postorId = null,
            CancellationToken ct = default);
        Task<SubastaDetalleDto?> ObtenerDetallePorIdAsync(int id, CancellationToken ct = default);
        Task<Subasta?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        Task<Subasta?> ObtenerConOfertasPorIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Subasta>> ObtenerVencidasParaLiquidacionAsync(DateTime ahora, CancellationToken ct = default);
        Task<IReadOnlyList<Subasta>> ObtenerProgramadasParaIniciarAsync(DateTime ahora, CancellationToken ct = default);
        Task AgregarAsync(Subasta subasta, CancellationToken ct = default);
        void Actualizar(Subasta subasta);
        Task<bool> CategoriaExisteAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<MiOfertaSubastaDto>> ObtenerMisOfertasAsync(int postorId, CancellationToken ct = default);
        Task<IReadOnlyList<MiPublicacionDto>> ObtenerMisPublicacionesAsync(int vendedorId, CancellationToken ct = default);
    }
}
