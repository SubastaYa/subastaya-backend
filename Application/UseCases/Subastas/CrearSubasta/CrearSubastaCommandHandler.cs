using Domain.Entities;
using Domain.Exceptions;

namespace Application.UseCases.Subastas.CrearSubasta
{
    public class CrearSubastaCommandHandler : ICommandHandler<CrearSubastaCommand, int>
    {
        private readonly ISubastaRepository _subastaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CrearSubastaCommandHandler(
            ISubastaRepository subastaRepository,
            IUnitOfWork unitOfWork)
        {
            _subastaRepository = subastaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> HandleAsync(CrearSubastaCommand command, CancellationToken ct = default)
        {
            var categoriaExiste = await _subastaRepository.CategoriaExisteAsync(command.CategoriaId, ct);
            if (!categoriaExiste)
            {
                throw new DomainValidationException($"La categoría con ID {command.CategoriaId} no existe.");
            }

            var subasta = Subasta.Crear(
                command.VendedorId,
                command.CategoriaId,
                command.Titulo,
                command.Descripcion,
                command.UrlImagen,
                command.PrecioBase,
                command.IncrementoMinimo,
                command.FechaInicio,
                command.FechaFin
            );

            await _subastaRepository.AgregarAsync(subasta, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return subasta.Id;
        }
    }
}
